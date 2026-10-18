#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Lumina.Domain.SharedKernel.Common.Enums.Playback;
using Lumina.Infrastructure.Common.Caching;
using Lumina.Infrastructure.Common.Models.DTO.Configuration;
using Lumina.Infrastructure.Common.Models.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.Core.MediaLibrary.AudioLibrary.MusicLibrary.Playback;

/// <summary>
/// Service that produces the normalized audio of a track with ffmpeg, caches it as fixed duration segments and serves it from the cache.
/// </summary>
/// <remarks>
/// The normalized audio is written to a temporary directory that is wiped when the application starts. The number of jobs that run at the same time is
/// bounded, and a request that finds every slot occupied is reported as busy instead of being queued, so client requests cannot accumulate behind the
/// running jobs. Every child process is stopped when the application exits, and every job has a bounded duration. A production is shared by every request
/// for the same track and delivery: a request that disconnects stops waiting but does not stop the production, so repeated aborts converge to a cached entry.
/// A production is atomic: the entry is published only after ffmpeg finishes the whole track, so the first request waits for a complete production; the
/// produced segments are then served from the cache without buffering them into memory. The total size of the cache, including the output of the jobs that
/// are still running, is accounted for by a process wide coordinator, which evicts the least recently used entries that are neither freshly produced nor
/// being read. The command line is always built as a list of arguments, with the input protocol restricted to the file protocol, so a file path is never
/// interpolated into a command string and a probed file can never fetch a remote URL. Every component of a source path below a content location is checked
/// for reparse points, and ffmpeg consumes a private snapshot copied from the opened handle, so a path that is swapped after the open cannot redirect the
/// bytes that are transcoded.
/// </remarks>
internal sealed class TrackNormalizationService : ITrackNormalizationService, IDisposable
{
    private const string CACHE_INDEX_FILE_NAME = "index.json";
    private const string SEGMENT_LIST_FILE_NAME = "segments.csv";
    private const string SOURCE_SNAPSHOT_FILE_NAME = "source";
    private const string SEGMENT_FILE_PREFIX = "segment-";
    private const string SEGMENT_FILE_NAME_PATTERN = SEGMENT_FILE_PREFIX + "%06d";
    private const string AUDIO_STREAM_MAP = "0:a:0";
    private const string NO_METADATA_MAP = "-1";
    private const string COPY_CODEC = "copy";
    private const string FILE_PROTOCOL = "file";
    private const long MAX_CACHE_INDEX_BYTES = 4 * 1024 * 1024;
    private const long MAX_SEGMENT_LIST_BYTES = 4 * 1024 * 1024;
    private const int CACHE_INDEX_VERSION = 2;
    private const int MAX_SEGMENT_COUNT = 1_000_000;
    private const int CACHE_SIZE_MONITOR_INTERVAL_MILLISECONDS = 500;
    private const int PROCESS_EXIT_TIMEOUT_SECONDS = 5;
    private const int PRODUCTION_DRAIN_TIMEOUT_SECONDS = 15;
    private const int MAX_STANDARD_ERROR_CHARACTERS = 16 * 1024;
    private const int AUTO_TRANSCODE_BITRATE_KBPS = 192;
    private const int DEFAULT_SOURCE_BITRATE_KBPS = 320;
    private const int DEFAULT_SAMPLE_RATE = 44100;
    private const int DEFAULT_CHANNEL_COUNT = 2;
    private const int DEFAULT_BIT_DEPTH = 16;
    private const decimal FLAC_COMPRESSION_RATIO = 0.7m;
    private const decimal REMUX_SIZE_MARGIN = 1.1m;
    private const decimal SEGMENT_TIME_TOLERANCE_SECONDS = 0.001m;
    private const decimal DURATION_TOLERANCE_MIN_SECONDS = 1m;
    private const decimal DURATION_TOLERANCE_RATIO = 0.02m;
    private const int MAX_TRACK_PROJECTIONS = 1024;

    // The demuxer of each container the probe can report. Pinning the demuxer stops ffmpeg from probing the content of the file and selecting a
    // demuxer that can open a playlist or another indirection; the file protocol whitelist is the backstop for a container that is not listed here.
    private static readonly FrozenDictionary<string, string> s_sourceDemuxers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["MP3"] = "mp3",
        ["FLAC"] = "flac",
        ["MP4"] = "mov",
        ["M4A"] = "mov",
        ["AAC"] = "aac",
        ["ADTS"] = "aac",
        ["OGG"] = "ogg",
        ["OPUS"] = "ogg",
        ["VORBIS"] = "ogg",
        ["WAV"] = "wav",
        ["WAVE"] = "wav",
        ["AIFF"] = "aiff",
        ["APE"] = "ape",
        ["WAVPACK"] = "wv",
        ["WV"] = "wv",
        ["WMA"] = "asf",
        ["ASF"] = "asf",
        ["DSF"] = "dsf",
        ["MATROSKA"] = "matroska",
        ["MKA"] = "matroska",
        ["WEBM"] = "matroska",
        ["MPC"] = "mpc",
        ["AA"] = "aa"
    }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    // The index is server written, but a tampered file is still parsed defensively: MaxDepth bounds the recursion of the deserializer.
    private static readonly JsonSerializerOptions s_indexJsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        MaxDepth = 8
    };

    // The productions are shared by every request of the process for the same cache entry, so a request that disconnects never cancels the work that
    // another request, or a later retry, is waiting for. The key includes the cache root, because two service instances configured with different cache
    // directories must never join each other's production. A completed production is removed from the registry; a later request reads the cache instead.
    private static readonly ConcurrentDictionary<(string CacheRoot, string CacheId), Lazy<Task<Result<NormalizedTrackDto>>>> s_normalizationJobs = new();

    private readonly TranscodingSettingsDto _settings;
    private readonly ILogger<TrackNormalizationService> _logger;
    private readonly IPathService _pathService;
    private readonly TranscodingCacheCoordinator _cache;
    private readonly TranscodingToolchainStatus _toolchainStatus;
    private readonly TimeSpan _evictionGracePeriod;
    private readonly TimeSpan _touchThrottleInterval;
    private readonly SemaphoreSlim _jobSlots;
    private readonly ConcurrentDictionary<int, Process> _runningProcesses = new();
    // The projection of a cache index is reused between segment requests, so a cached track is not re-projected for every segment. The bound keeps
    // the retained metadata small; a cleared cache is repopulated lazily from the index on the next request.
    private readonly ConcurrentDictionary<(string CacheRoot, string CacheId), NormalizedTrackDto> _trackProjections = new();
    private readonly CancellationTokenSource _shutdownCancellation = new();
    private readonly Lock _lifecycleSync = new();
    // The global job slots are shared, but one user must not be able to take all of them and block another user's playback. These two fields guard
    // the per-user admission count; they are not part of the cache identity, because a production is shared by every user that requests it.
    private readonly Lock _userProductionLock = new();
    private readonly Dictionary<Guid, int> _activeProductionsPerUser = [];
    private readonly ManualResetEventSlim _productionsDrained = new(initialState: true);
    private readonly string _cacheRoot;
    private int _activeProductionCount;
    // Written under _lifecycleSync by Dispose and read without the lock by the request paths through Volatile.Read, so the write is observed promptly.
    private bool _isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="TrackNormalizationService"/> class.
    /// </summary>
    /// <param name="options">The audio normalization configuration options.</param>
    /// <param name="logger">The logger for this service.</param>
    /// <param name="pathService">The service used to validate that a cached segment is inside its cache entry.</param>
    /// <param name="toolchainStatus">The status that records whether the ffmpeg toolchain found at startup can produce a format.</param>
    public TrackNormalizationService(IOptions<TranscodingSettingsDto> options, ILogger<TrackNormalizationService> logger, IPathService pathService, TranscodingToolchainStatus toolchainStatus)
    {
        _settings = options.Value;
        _logger = logger;
        _pathService = pathService;
        _toolchainStatus = toolchainStatus;
        _jobSlots = new SemaphoreSlim(_settings.MaxConcurrentJobs, _settings.MaxConcurrentJobs);
        _evictionGracePeriod = TimeSpan.FromSeconds(_settings.EvictionGraceSeconds);
        // A touch is written at most every quarter of the eviction grace period, so a stream that opens many segments does not rewrite the cache
        // entry for every request while an actively used entry stays protected from eviction.
        _touchThrottleInterval = TimeSpan.FromSeconds(Math.Clamp(_settings.EvictionGraceSeconds / 4.0, 0.25, 30));
        _cacheRoot = TranscodingCachePaths.GetRootDirectory(_settings.CacheDirectory);
        _cache = TranscodingCacheCoordinator.AcquireForRoot(_cacheRoot);
        // A killed process must never outlive the application, so every child process that is still running is stopped when the process exits. The
        // subscription is removed when the service is disposed, so a host that is shut down in the same process does not keep this instance rooted.
        AppDomain.CurrentDomain.ProcessExit += OnApplicationStopping;
    }

    /// <summary>
    /// Gets the normalized audio of a track from the cache, or produces it and caches it when it is not cached yet.
    /// </summary>
    /// <param name="request">The track, the delivery decision, the source file and the quality cap the normalized audio is produced with.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the cached normalized audio of the track, or an error message.</returns>
    /// <exception cref="OperationCanceledException">
    /// Thrown when <paramref name="cancellationToken"/> is canceled while the caller is waiting. The production continues in the background, so this is a
    /// disconnect signal and must not be treated as a production failure.
    /// </exception>
    public async Task<Result<NormalizedTrackDto>> GetOrCreateNormalizedTrackAsync(NormalizedTrackRequestDto request, CancellationToken cancellationToken)
    {
        // Dispose sets _isDisposed under _lifecycleSync, but taking that lock on every request would serialize this hot path. The flag is therefore read
        // without the lock. Volatile.Read keeps the read from being cached in a register, so a request observes the disposal flag once it is set and
        // fails fast instead of using resources that Dispose has already released.
        if (Volatile.Read(ref _isDisposed))
            return Errors.Playback.NormalizationUnavailable;
        if (request.TrackId == Guid.Empty)
            return Errors.Playback.NormalizationFailed;
        if (request.UserId == Guid.Empty)
            return Errors.Playback.NormalizationFailed;
        if (string.IsNullOrWhiteSpace(request.SourcePath))
            return Errors.Music.TrackPathCannotBeEmpty;
        // The caller authorizes the request, and the service re-validates the source path against the content locations carried by the request. A path
        // that is outside every allowed root is reported as not found, never as a path error, so no file system detail is disclosed.
        if (request.AllowedRoots is null || request.AllowedRoots.Count == 0 || !IsPathAllowed(request.SourcePath, request.AllowedRoots))
            return Errors.Music.TrackFileNotFound;
        // Normalization only exists for audio the player cannot play as stored, so a direct play never reaches the pipeline.
        if (request.Mode == PlaybackDeliveryMode.DirectPlay)
            return Errors.Playback.NormalizationIsNotRequiredForDirectPlay;
        if (request.SegmentDurationSeconds <= 0)
            return Errors.Playback.SegmentDurationMustBePositive;
        if (request.DurationInSeconds <= 0)
            return Errors.Playback.TrackDurationMustBePositive;
        // A delivery decision that names a pair the catalog does not implement is a disagreement between two server side tables. It must never be
        // reported as a limitation of the player, which is what the forbidden NoSupportedPlaybackFormat means.
        if (!PlaybackFormatCatalog.FormatsByPair.TryGetValue(PlaybackFormatCatalog.BuildPairKey(request.Container, request.Codec), out NormalizedAudioFormatDto? format))
            return Errors.Playback.PlaybackFormatNotImplemented;

        FileInfo sourceFile;
        long sourceLength;
        DateTime sourceLastModifiedUtc;
        try
        {
            sourceFile = new FileInfo(request.SourcePath);
            if (!sourceFile.Exists)
                return Errors.Music.TrackFileNotFound;
            // The length and the modification time of the source are read once, while the file is known to exist, and are used both for the cache
            // identity and for the size estimate, so a file that disappears between two reads can never surface as an unhandled exception.
            sourceLength = sourceFile.Length;
            sourceLastModifiedUtc = sourceFile.LastWriteTimeUtc;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or NotSupportedException)
        {           
            return Errors.Music.TrackFileNotFound; // A file that cannot be read is indistinguishable, to the caller, from a file that does not exist.
        }

        // The delivery was declared against a specific version of the source file. A file that changed since then would produce a new cache
        // entry with a different identity, so its segments would be mixed with the segments of the declared delivery. It is rejected instead.
        if (request.SourceLength != sourceLength || request.SourceLastModifiedUtc != sourceLastModifiedUtc)
            return Errors.Playback.PlaybackDeliveryChanged;

        string cacheId = BuildCacheId(request, sourceFile.FullName, sourceLength, sourceLastModifiedUtc, format);
        // The entry is touched before its index is read, so that a cache hit can never race an eviction that is deciding on the entry at the same time:
        // an eviction that runs after the touch sees the entry as recently used, and one that ran before the touch makes the read miss and produce anew.
        _cache.Touch(cacheId, _touchThrottleInterval);
        if (TryReadCachedTrack(cacheId, out NormalizedTrackDto? cachedTrack) && cachedTrack is not null && cachedTrack.TrackId == request.TrackId)
            return cachedTrack;

        // The production is shared by every request for this cache entry and is not tied to this request's token. This request only waits for it, so a
        // disconnect stops this caller from waiting without destroying the work, and the next request either joins the same production or reads its result.
        (string CacheRoot, string CacheId) normalizationJobKey = (_cacheRoot, cacheId);
        // ExecutionAndPublication makes the factory run exactly once when several requests race to create the same job.
        Lazy<Task<Result<NormalizedTrackDto>>> normalizationJob = s_normalizationJobs.GetOrAdd(
            normalizationJobKey,
            createdJobKey => new Lazy<Task<Result<NormalizedTrackDto>>>(
                () => ProduceAsync(cacheId, request, format, sourceLength, sourceLastModifiedUtc),
                LazyThreadSafetyMode.ExecutionAndPublication));
        Task<Result<NormalizedTrackDto>> normalizationTask = normalizationJob.Value;
        // Remove the job from the static registry when it finishes, so a completed result is not kept in memory forever.
        _ = normalizationTask.ContinueWith(
            completedTask => RemoveNormalizationJob(normalizationJobKey, normalizationJob),
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);

        return await normalizationTask.WaitAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Opens a read stream over a single segment of a cached normalized track, after verifying that the entry belongs to the provided track.
    /// </summary>
    /// <param name="cacheId">The opaque identifier of the cached entry that holds the normalized audio.</param>
    /// <param name="trackId">The Id of the track the caller authorized and whose segment is opened.</param>
    /// <param name="segmentIndex">The zero based position of the segment in the delivered audio.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a read stream over the segment, or an error message. The caller owns the returned stream.
    /// </returns>
    public Result<Stream> OpenNormalizedSegment(string cacheId, Guid trackId, int segmentIndex)
    {
        // This runs for every segment request and must not take _lifecycleSync. Dispose writes _isDisposed under that lock, and Volatile.Read makes this
        // lock free check observe the write instead of a value cached in a register, so segments stop being served as soon as the service is disposed.
        if (Volatile.Read(ref _isDisposed))
            return Errors.Playback.NormalizationUnavailable;
        if (trackId == Guid.Empty || !TranscodingCacheCoordinator.IsValidCacheId(cacheId) || segmentIndex < 0)
            return Errors.Playback.NormalizedTrackCacheEntryNotFound;

        // The lease is acquired before the index is read, so that an eviction that runs concurrently can never delete the entry between the read and
        // the moment the segment stream starts. The lease is handed over to the stream and released when the caller disposes it.
        TranscodingCacheReaderLease lease = _cache.AcquireReaderLease(cacheId);
        bool isLeaseTransferred = false;
        try
        {
            if (!TryReadIndex(cacheId, out NormalizedTrackIndexDto? index) || index is null)
                return Errors.Playback.NormalizedTrackCacheEntryNotFound;

            // The cache handle is not a capability: it is derived from the track and can be computed by anyone who knows the track. The entry is bound
            // to the track it was produced for, so a handle can only ever serve the track the caller authorized against the route.
            if (index.TrackId != trackId)
                return Errors.Music.TrackNotFound;

            NormalizedTrackSegmentIndexDto? segment = index.Segments.FirstOrDefault(candidate => candidate.Index == segmentIndex);
            if (segment is null)
                return Errors.Playback.NormalizedTrackCacheEntryNotFound;

            string cachePath = Path.Combine(_cacheRoot, cacheId);
            string segmentPath = Path.Combine(cachePath, segment.FileName);
            // The file name comes from the cache index, which only the server writes, but the resolved path is still confined to the cache entry so that a
            // tampered index can never be used to read a file outside it.
            if (!_pathService.IsPathWithin(segmentPath, cachePath))
                return Errors.Playback.NormalizedTrackCacheEntryNotFound;
            // A cache entry is never reached through a reparse point. The index is only written by this application, but a tampered cache entry must not be
            // able to redirect a segment outside the cache root.
            if (HasReparsePointBelow(_cacheRoot, segmentPath))
                return Errors.Playback.NormalizedTrackCacheEntryNotFound;

            FileInfo segmentFile = new(segmentPath);
            if (!segmentFile.Exists || segmentFile.Length != segment.Length)
                return Errors.Playback.NormalizedTrackCacheEntryNotFound;

            _cache.Touch(cacheId, _touchThrottleInterval);
            // A published entry is immutable, so every reader can share the file. Delete sharing is not granted: replacement is coordinated through the
            // lease and the publication lock, never by deleting a file under a reader.
            FileStream fileStream = new(segmentPath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);
            try
            {
                // The handle is checked against the length from the index and against the reparse points again, so a file or an ancestor swapped between
                // the check and the open is never handed to the caller.
                if (fileStream.Length != segment.Length || HasReparsePointBelow(_cacheRoot, segmentPath))
                {
                    fileStream.Dispose();
                    return Errors.Playback.NormalizedTrackCacheEntryNotFound;
                }
            }
            catch
            {
                fileStream.Dispose();
                throw;
            }
            TranscodingCacheLeaseStream leaseStream = new(fileStream, lease);
            isLeaseTransferred = true;
            return Result<Stream>.Success(leaseStream);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or NotSupportedException)
        {
            return Errors.Playback.NormalizedTrackCacheEntryNotFound;
        }
        finally
        {
            if (!isLeaseTransferred)
                lease.Dispose();
        }
    }

    /// <summary>
    /// Opens the stored audio file of a track after re-validating that it is inside an allowed root, and re-checks the opened handle.
    /// </summary>
    /// <remarks>
    /// The caller has already authorized the request, but the path is still validated here as defense in depth. Every component between an allowed root and
    /// the leaf is rejected when it is a reparse point, so a symbolic link inside a content location can never redirect the file outside it. The file is then
    /// opened, and the opened handle is checked against the allowed roots and against the length and modification time the cache identity was built from.
    /// The caller copies the opened handle into a private snapshot before the transcoder runs, so the transcoder reads exactly the bytes that were validated,
    /// even when the path is replaced afterwards. The length is read from the handle; the modification time is a path based, best effort comparison.
    /// </remarks>
    /// <param name="request">The request whose source path and allowed roots are validated.</param>
    /// <param name="sourceLength">The length of the stored audio file of the track, in bytes, read when the cache identity was built.</param>
    /// <param name="sourceLastModifiedUtc">The date and time, in UTC, when the stored audio file of the track was last modified, read when the cache identity was built.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either an open read stream over the authorized source file, or an error message. The caller owns the stream.</returns>
    private Result<FileStream> OpenAuthorizedSource(NormalizedTrackRequestDto request, long sourceLength, DateTime sourceLastModifiedUtc)
    {
        try
        {
            FileInfo sourceFile = new(request.SourcePath);
            if (!sourceFile.Exists || !IsPathAllowed(sourceFile.FullName, request.AllowedRoots))
                return Errors.Music.TrackFileNotFound;

            string resolvedPath = sourceFile.FullName;
            FileStream sourceStream = new(resolvedPath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);
            try
            {
                // The file is checked again after it has been opened, so a path that was swapped between the FileInfo read and the open is never handed
                // to the transcoder. The length is read from the handle itself; the modification time must also still match the value the cache identity was built from.
                FileInfo openedFile = new(resolvedPath);
                if (!openedFile.Exists ||
                    !IsPathAllowed(openedFile.FullName, request.AllowedRoots) ||
                    sourceStream.Length != sourceLength ||
                    openedFile.LastWriteTimeUtc != sourceLastModifiedUtc)
                {
                    sourceStream.Dispose();
                    return Errors.Music.TrackFileNotFound;
                }

                return sourceStream;
            }
            catch
            {
                sourceStream.Dispose();
                throw;
            }
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or NotSupportedException)
        {
            return Errors.Music.TrackFileNotFound;
        }
    }

    /// <summary>
    /// Checks whether a path is inside at least one of the allowed roots, without crossing a reparse point below that root.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <param name="allowedRoots">The roots the path is allowed to be inside.</param>
    /// <returns><see langword="true"/> when the path is inside an allowed root and no component below that root is a reparse point, otherwise <see langword="false"/>.</returns>
    private bool IsPathAllowed(string path, IReadOnlyList<string> allowedRoots)
    {
        foreach (string allowedRoot in allowedRoots)
        {
            if (string.IsNullOrWhiteSpace(allowedRoot) || !_pathService.IsPathWithin(path, allowedRoot))
                continue;
            // A reparse point inside the content location fails closed instead of being resolved, because its target is outside the root the caller authorized.
            return !HasReparsePointBelow(allowedRoot, path);
        }
        return false;
    }

    /// <summary>
    /// Checks whether any component of a path below a root is a reparse point.
    /// </summary>
    /// <param name="rootPath">The root below which the components are checked.</param>
    /// <param name="path">The path inside the root whose components are checked.</param>
    /// <returns><see langword="true"/> when a component below the root is a reparse point, otherwise <see langword="false"/>.</returns>
    private static bool HasReparsePointBelow(string rootPath, string path)
    {
        string normalizedRootPath = Path.GetFullPath(rootPath);
        string normalizedPath = Path.GetFullPath(path);
        string relativePath = Path.GetRelativePath(normalizedRootPath, normalizedPath);
        if (relativePath == ".")
            return false;
        string currentPath = normalizedRootPath;
        foreach (string segment in relativePath.Split([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar], StringSplitOptions.RemoveEmptyEntries))
        {
            // The lexical containment check already succeeded, so a relative escape means the two paths disagree; the file is rejected rather than trusted.
            if (segment == "..")
                return true;
            if (segment == ".")
                continue;
            currentPath = Path.Combine(currentPath, segment);
            if ((File.GetAttributes(currentPath) & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Admits a production to the service lifetime, so that shutdown cancels it and disposes the process lifetime resources only after it has left.
    /// </summary>
    /// <param name="cacheId">The opaque identifier of the cache entry.</param>
    /// <param name="request">The track, the delivery decision and the quality cap the normalized audio is produced with.</param>
    /// <param name="format">The container and codec combination the normalized audio is produced in.</param>
    /// <param name="sourceLength">The length of the stored audio file of the track, in bytes.</param>
    /// <param name="sourceLastModifiedUtc">The date and time, in UTC, when the stored audio file of the track was last modified.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the normalized audio of the track, or an error message.</returns>
    private async Task<Result<NormalizedTrackDto>> ProduceAsync(string cacheId, NormalizedTrackRequestDto request, NormalizedAudioFormatDto format, long sourceLength, DateTime sourceLastModifiedUtc)
    {
        // The global job slots are shared by every user, so a user who starts many productions at once could occupy all of them and make
        // another user's playback fail. The per-user admission happens before the production is registered, and a request that joins a
        // production another user already started does not reach this method, so a shared production is never charged twice.
        if (!TryAcquireUserProductionSlot(request.UserId))
            return Errors.Playback.NormalizationUnavailable;

        try
        {
            lock (_lifecycleSync)
            {
                // A production that arrives after shutdown has started must not touch the cancellation source, the job slots or the coordinator, because shutdown may already have disposed them.
                if (_isDisposed)
                    return Errors.Playback.NormalizationUnavailable;
                // Dispose waits on _productionsDrained before it releases the coordinator and job slots. Counting the production here closes the race
                // between the disposal check above and the resources being released while the production is still using them.
                _activeProductionCount++;
                if (_activeProductionCount == 1)
                    _productionsDrained.Reset();
            }

            try
            {
                return await ProduceCoreAsync(cacheId, request, format, sourceLength, sourceLastModifiedUtc).ConfigureAwait(false);
            }
            finally
            {
                lock (_lifecycleSync)
                {
                    _activeProductionCount--;
                    if (_activeProductionCount == 0)
                        _productionsDrained.Set();
                }
            }
        }
        finally
        {
            ReleaseUserProductionSlot(request.UserId);
        }
    }

    /// <summary>
    /// Reserves one of the per-user production slots before a production is admitted, so one user can never occupy every global job slot.
    /// </summary>
    /// <param name="userId">The Id of the user whose request starts the production.</param>
    /// <returns><see langword="true"/> when the slot was reserved, <see langword="false"/> otherwise.</returns>
    private bool TryAcquireUserProductionSlot(Guid userId)
    {
        lock (_userProductionLock)
        {
            if (userId == Guid.Empty)
                return false;
            int activeProductions = _activeProductionsPerUser.GetValueOrDefault(userId);
            if (activeProductions >= _settings.MaxConcurrentJobsPerUser)
                return false;
            _activeProductionsPerUser[userId] = activeProductions + 1;
            return true;
        }
    }

    /// <summary>
    /// Releases a per-user production slot that was reserved by <see cref="TryAcquireUserProductionSlot"/>.
    /// </summary>
    /// <param name="userId">The Id of the user whose production ended.</param>
    private void ReleaseUserProductionSlot(Guid userId)
    {
        lock (_userProductionLock)
        {
            if (!_activeProductionsPerUser.TryGetValue(userId, out int activeProductions))
                return;
            if (activeProductions <= 1)
                _activeProductionsPerUser.Remove(userId);
            else
                _activeProductionsPerUser[userId] = activeProductions - 1;
        }
    }

    /// <summary>
    /// Produces the normalized audio of a track in the background, independent of any request that waits for it.
    /// </summary>
    /// <param name="cacheId">The opaque identifier of the cache entry.</param>
    /// <param name="request">The track, the delivery decision and the quality cap the normalized audio is produced with.</param>
    /// <param name="format">The container and codec combination the normalized audio is produced in.</param>
    /// <param name="sourceLength">The length of the stored audio file of the track, in bytes.</param>
    /// <param name="sourceLastModifiedUtc">The date and time, in UTC, when the stored audio file of the track was last modified.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the normalized audio of the track, or an error message.</returns>
    private async Task<Result<NormalizedTrackDto>> ProduceCoreAsync(string cacheId, NormalizedTrackRequestDto request, NormalizedAudioFormatDto format, long sourceLength, DateTime sourceLastModifiedUtc)
    {
        CancellationToken cancellationToken = _shutdownCancellation.Token;

        // The startup initializer owns the cache directory, but the service re-ensures it before every production so a directory removed at runtime or a
        // failed startup wipe cannot turn into an unhandled exception or a partial write.
        try
        {
            TranscodingCachePaths.EnsureDirectoryExists(_cacheRoot);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            _logger.LogError(exception, "The normalized audio cache directory '{CacheDirectory}' is not available for track {TrackId} in cache entry {CacheId}.", _cacheRoot, request.TrackId, cacheId);
            return Errors.Playback.NormalizationUnavailable;
        }

        try
        {
            // A production is admitted immediately or reported as busy. Queueing a task per distinct request would let a client saturate the pipeline
            // and outlive its own timeout, while a started production is still allowed to outlive every disconnected waiter.
            if (!_jobSlots.Wait(0))
                return Errors.Playback.NormalizationUnavailable;
            try
            {
                // Another production for the same entry may have finished while this one was created, so the cache is checked again before spendingan admitted slot on work that is already done.
                if (TryReadCachedTrack(cacheId, out NormalizedTrackDto? cachedTrack) && cachedTrack is not null && cachedTrack.TrackId == request.TrackId)
                {
                    _cache.Touch(cacheId, _touchThrottleInterval);
                    return cachedTrack;
                }

                // A cached entry is served even when the toolchain is broken, but a new production needs a usable ffmpeg and, for a transcode, the
                // encoder of the delivery format. The startup probe recorded what is missing, so this is reported as unavailable instead of failing per track.
                if (!_toolchainStatus.IsTranscoderAvailable || (request.Mode == PlaybackDeliveryMode.Transcode && !_toolchainStatus.IsEncoderAvailable(format.EncoderName)))
                    return Errors.Playback.NormalizationUnavailable;

                long estimatedSize = EstimateOutputSize(request, format);
                if (!_cache.TryReserve(estimatedSize, _settings.MaxCacheBytes, _evictionGracePeriod, out TranscodingCacheReservation? reservation) || reservation is null)
                    return Errors.Playback.NormalizationCacheLimitExceeded;

                using (reservation)
                    return await NormalizeAsync(cacheId, request, format, sourceLength, sourceLastModifiedUtc, reservation, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                _jobSlots.Release();
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {           
            return Errors.Playback.NormalizationUnavailable; // The application is stopping, so the production cannot finish; it is reported instead of being left as an unobserved cancellation.
        }
        catch (Exception exception)
        {
            // The production is shared by requests that are no longer necessarily waiting, so every failure is turned into a result and observed here.
            _logger.LogError(exception, "The normalized audio of track {TrackId} in cache entry {CacheId} could not be produced.", request.TrackId, cacheId);
            return Errors.Playback.NormalizationFailed;
        }
    }

    /// <summary>
    /// Produces the normalized audio of a track into a temporary directory, persists its index and asks the coordinator to publish it into the cache.
    /// </summary>
    /// <param name="cacheId">The opaque identifier of the cache entry.</param>
    /// <param name="request">The track, the delivery decision and the quality cap the normalized audio is produced with.</param>
    /// <param name="format">The container and codec combination the normalized audio is produced in.</param>
    /// <param name="sourceLength">The length of the stored audio file of the track, in bytes.</param>
    /// <param name="sourceLastModifiedUtc">The date and time, in UTC, when the stored audio file of the track was last modified.</param>
    /// <param name="reservation">The capacity the job reserved in the cache.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the normalized audio of the track, or an error message.</returns>
    private async Task<Result<NormalizedTrackDto>> NormalizeAsync(string cacheId, NormalizedTrackRequestDto request, NormalizedAudioFormatDto format, long sourceLength, DateTime sourceLastModifiedUtc, TranscodingCacheReservation reservation, CancellationToken cancellationToken)
    {
        string temporaryPath = Path.Combine(_cacheRoot, TranscodingCachePaths.TEMPORARY_DIRECTORY_PREFIX + Guid.NewGuid().ToString("N"));
        string segmentListPath = Path.Combine(temporaryPath, SEGMENT_LIST_FILE_NAME);

        // The source is opened and re-validated here, immediately before the transcoder runs, and copied into a private snapshot that the transcoder reads.
        // The snapshot is written from the validated handle, so the transcoder consumes exactly the bytes that were validated even when the path is replaced.
        Result<FileStream> openResult = OpenAuthorizedSource(request, sourceLength, sourceLastModifiedUtc);
        if (openResult.IsFailure)
            return openResult.Errors;

        using (FileStream sourceStream = openResult.Value)
        {
            try
            {
                TranscodingCachePaths.CreatePrivateDirectory(temporaryPath);
                string snapshotPath = Path.Combine(temporaryPath, SOURCE_SNAPSHOT_FILE_NAME);
                bool isSnapshotComplete;
                // The copy is bounded by the job timeout as well, so a source that never finishes reading cannot hold a job slot indefinitely.
                using (CancellationTokenSource snapshotTimeoutCancellationTokenSource = new(TimeSpan.FromSeconds(_settings.JobTimeoutSeconds)))
                {
                    using (CancellationTokenSource snapshotCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, snapshotTimeoutCancellationTokenSource.Token))
                    {
                        try
                        {
                            isSnapshotComplete = await CopySourceSnapshotAsync(sourceStream, snapshotPath, sourceLength, snapshotCancellationTokenSource.Token).ConfigureAwait(false);
                        }
                        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                        {
                            return Errors.Playback.NormalizationTimedOut;
                        }
                    }
                }
                if (!isSnapshotComplete)
                    return Errors.Music.TrackFileNotFound;

                Result<NormalizedTrackIndexDto> runResult;
                try
                {
                    runResult = await RunFfmpegAsync(cacheId, request, snapshotPath, format, temporaryPath, segmentListPath, reservation, cancellationToken).ConfigureAwait(false);
                }
                finally
                {
                    // The snapshot is not part of the cache entry; it is removed before the entry is moved or the temporary directory is cleaned.
                    TryDeleteFile(snapshotPath);
                }
                if (runResult.IsFailure)
                    return runResult.Errors;
                NormalizedTrackIndexDto index = runResult.Value;
                // The csv is only an intermediate of the production; it is not part of the published entry and is never read again.
                TryDeleteFile(segmentListPath);
                // The entries are created under a private root, but each produced file is tightened as well instead of relying on the directory mode alone.
                foreach (NormalizedTrackSegmentIndexDto segment in index.Segments)
                    TranscodingCachePaths.RestrictFilePermissions(Path.Combine(temporaryPath, segment.FileName));

                // The index is written before the entry is published, so a cache hit never has to parse the ffmpeg output or probe the segments again.
                string indexJson = JsonSerializer.Serialize(index, s_indexJsonOptions);
                string temporaryIndexPath = Path.Combine(temporaryPath, CACHE_INDEX_FILE_NAME);
                await File.WriteAllTextAsync(temporaryIndexPath, indexJson, cancellationToken).ConfigureAwait(false);
                TranscodingCachePaths.RestrictFilePermissions(temporaryIndexPath);
                // The segments were stat'ed by the index parser, so the size is the parsed lengths plus the index file instead of another walk of the entry.
                long entrySize = index.Segments.Sum(segment => segment.Length) + Encoding.UTF8.GetByteCount(indexJson);
                FileInfo remainingSegmentList = new(segmentListPath);
                if (remainingSegmentList.Exists)
                    entrySize += remainingSegmentList.Length;
                // The coordinator replaces the previous directory, moves this one into place and registers its index under one lock, so a concurrent
                // eviction can never delete the freshly published entry and a reader is never served a mix of two productions.
                if (!reservation.TryComplete(cacheId, temporaryPath, entrySize, index, _settings.MaxCacheBytes, _evictionGracePeriod, out bool isKeptOverCapacity, out bool isEntryInUse))
                {
                    if (isEntryInUse)
                        return Errors.Playback.NormalizationUnavailable;
                    return Errors.Playback.NormalizationFailed;
                }
                if (isKeptOverCapacity)
                    _logger.LogWarning("The normalized audio of track {TrackId} in cache entry {CacheId} was produced successfully but exceeds the configured normalized audio cache limit. It is kept readable and will be evicted when it is no longer in use.", request.TrackId, cacheId);

                return GetOrCreateTrackProjection(cacheId, index);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException or NotSupportedException)
            {
                _logger.LogWarning(exception, "The normalized audio of track {TrackId} in cache entry {CacheId} could not be cached.", request.TrackId, cacheId);
                return Errors.Playback.NormalizationFailed;
            }
            finally
            {
                if (Directory.Exists(temporaryPath))
                    TranscodingCachePaths.TryDeleteDirectory(temporaryPath);
            }
        }
    }

    /// <summary>
    /// Copies the validated source stream into a private snapshot file, so the transcoder reads exactly the bytes that were validated.
    /// </summary>
    /// <param name="sourceStream">The open, validated stream over the stored audio file of the track.</param>
    /// <param name="snapshotPath">The path of the snapshot file to create.</param>
    /// <param name="expectedLength">The expected length of the source file, in bytes.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns><see langword="true"/> when the whole source was copied, otherwise <see langword="false"/>.</returns>
    private static async Task<bool> CopySourceSnapshotAsync(FileStream sourceStream, string snapshotPath, long expectedLength, CancellationToken cancellationToken)
    {
        using (FileStream snapshotStream = new(snapshotPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true))
            await sourceStream.CopyToAsync(snapshotStream, cancellationToken).ConfigureAwait(false);
        // The snapshot is read from the open handle, so the comparison detects a source that was truncated or grew after the handle was validated.
        return new FileInfo(snapshotPath).Length == expectedLength;
    }

    /// <summary>
    /// Runs ffmpeg to write the normalized audio of a track into a temporary directory, and builds the index of the produced segments.
    /// </summary>
    /// <param name="request">The track, the delivery decision and the quality cap the normalized audio is produced with.</param>
    /// <param name="inputPath">The path of the private snapshot of the stored audio file, which was copied from the validated handle.</param>
    /// <param name="format">The container and codec combination the normalized audio is produced in.</param>
    /// <param name="temporaryPath">The temporary directory the normalized audio is written into.</param>
    /// <param name="segmentListPath">The file into which ffmpeg writes the list of produced segments.</param>
    /// <param name="reservation">The capacity the job reserved in the cache.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the index of the produced segments, or an error message.</returns>
    private async Task<Result<NormalizedTrackIndexDto>> RunFfmpegAsync(string cacheId, NormalizedTrackRequestDto request, string inputPath, NormalizedAudioFormatDto format, string temporaryPath, string segmentListPath, TranscodingCacheReservation reservation, CancellationToken cancellationToken)
    {
        using Process process = new();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = _settings.FfmpegPath,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardError = true,
            RedirectStandardOutput = false
        };
        foreach (string argument in BuildArguments(request, inputPath, format, temporaryPath, segmentListPath))
            process.StartInfo.ArgumentList.Add(argument);

        try
        {
            if (!process.Start())
                return Errors.Playback.NormalizationFailed;
        }
        catch (Exception exception) when (exception is Win32Exception or InvalidOperationException or IOException)
        {
            _logger.LogError(exception, "The audio transcoder for track {TrackId} in cache entry {CacheId} could not be started.", request.TrackId, cacheId);
            return Errors.Playback.NormalizationUnavailable;
        }

        // The process is registered so the application exit handler can kill it even if the host stops without reaching this method's finally.
        _runningProcesses[process.Id] = process;
        try
        {
            // Start draining stderr before waiting. A child that fills the pipe buffer blocks until it is read, so waiting for the exit first could deadlock.
            Task<string> standardErrorTask = ReadBoundedStandardErrorAsync(process.StandardError);
            using CancellationTokenSource timeoutCancellationTokenSource = new(TimeSpan.FromSeconds(_settings.JobTimeoutSeconds));
            using CancellationTokenSource linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCancellationTokenSource.Token);
            // The callback kills ffmpeg as soon as the token fires, so a cancellation or timeout stops the child even while this thread is awaiting its
            // exit. The catch below only observes the exit that the callback caused.
            using CancellationTokenRegistration cancellationRegistration = linkedCancellationTokenSource.Token.Register(() => KillProcess(process));
            using CancellationTokenSource monitorCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(linkedCancellationTokenSource.Token);
            TranscodingOutputSizeTracker outputSizeTracker = new(temporaryPath, SEGMENT_FILE_PREFIX, format.FileExtension, SEGMENT_LIST_FILE_NAME);
            Task<bool> monitorTask = MonitorOutputSizeAsync(cacheId, request.TrackId, outputSizeTracker, reservation, process, monitorCancellationTokenSource.Token);
            try
            {
                await process.WaitForExitAsync(linkedCancellationTokenSource.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                KillProcess(process);
                // The cancellation callback fired the kill without waiting. The exit is observed asynchronously here, so the temporary directory is
                // cleaned up after the child has actually stopped, without pinning the thread that triggered the cancellation.
                using CancellationTokenSource exitCancellationTokenSource = new(TimeSpan.FromSeconds(PROCESS_EXIT_TIMEOUT_SECONDS));
                try
                {
                    await process.WaitForExitAsync(exitCancellationTokenSource.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogDebug("The audio transcoder did not exit within the process exit timeout.");
                }
            }
            finally
            {
                // The monitor loops until it is canceled. Stop it before awaiting it, or a job that finished normally would wait forever.
                monitorCancellationTokenSource.Cancel();
            }

            bool isCacheLimitExceeded = await monitorTask.ConfigureAwait(false);
            // The read is bounded: a process that was killed but did not exit would otherwise keep the pipe open and park the job slot.
            string standardError = await AwaitStandardErrorAsync(standardErrorTask).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            if (isCacheLimitExceeded)
                return Errors.Playback.NormalizationCacheLimitExceeded;
            if (timeoutCancellationTokenSource.IsCancellationRequested)
                return Errors.Playback.NormalizationTimedOut;
            if (process.ExitCode != 0)
            {
                _logger.LogWarning("The audio transcoder for track {TrackId} in cache entry {CacheId} exited with code {ExitCode}.", request.TrackId, cacheId, process.ExitCode);
                if (!string.IsNullOrWhiteSpace(standardError))
                {
                    // The bounded stderr can contain the source path and media metadata. It is only written at Debug and never returned to a client.
                    string diagnostic = RedactPath(RedactPath(standardError, inputPath), request.SourcePath);
                    _logger.LogDebug("The audio transcoder for track {TrackId} in cache entry {CacheId} reported: {StandardError}", request.TrackId, cacheId, diagnostic);
                }
                return Errors.Playback.NormalizationFailed;
            }

            return ParseSegmentIndex(cacheId, segmentListPath, temporaryPath, request, format);
        }
        finally
        {
            _runningProcesses.TryRemove(process.Id, out _);
            KillProcess(process);
        }
    }

    /// <summary>
    /// Builds the argument list of a single ffmpeg invocation, with one argument per list entry, so a path is never interpolated into a command line.
    /// </summary>
    /// <param name="request">The track, the delivery decision and the quality cap the normalized audio is produced with.</param>
    /// <param name="inputPath">The path of the private snapshot of the stored audio file, which was copied from the validated handle.</param>
    /// <param name="format">The container and codec combination the normalized audio is produced in.</param>
    /// <param name="temporaryPath">The temporary directory the normalized audio is written into.</param>
    /// <param name="segmentListPath">The file into which ffmpeg writes the list of produced segments.</param>
    /// <returns>The argument list of the ffmpeg invocation.</returns>
    private static IReadOnlyList<string> BuildArguments(NormalizedTrackRequestDto request, string inputPath, NormalizedAudioFormatDto format, string temporaryPath, string segmentListPath)
    {
        List<string> arguments =
        [
            "-hide_banner",
            "-loglevel", "error",
            "-nostdin",
            "-y",
            "-protocol_whitelist", FILE_PROTOCOL
        ];

        // The demuxer is pinned to the container the probe stored, so ffmpeg never probes the content of the file and never selects a demuxer that
        // can follow a playlist or another indirection. A container that is not known is still limited to the file protocol by the whitelist above.
        if (TryResolveSourceDemuxer(request.SourceContainer, out string sourceDemuxer))
        {
            arguments.Add("-f");
            arguments.Add(sourceDemuxer);
        }

        arguments.AddRange(
        [
            "-i", inputPath,
            "-map", AUDIO_STREAM_MAP,
            "-vn",
            "-sn",
            "-dn",
            "-map_metadata", NO_METADATA_MAP
        ]);

        // A remux only repackages the stored codec, while a transcode re-encodes it and applies the streaming quality cap when the codec is lossy.
        arguments.Add("-c:a");
        if (request.Mode == PlaybackDeliveryMode.Remux)
            arguments.Add(COPY_CODEC);
        else
        {
            arguments.Add(format.EncoderName);
            // A lossy transcode always carries an explicit bitrate, including Auto, so the emitted stream and the cache size estimate use the same number.
            if (!format.IsLossless)
            {
                arguments.Add("-b:a");
                arguments.Add(GetTargetBitrateKbps(request.Quality).ToString(CultureInfo.InvariantCulture) + "k");
            }
        }

        arguments.AddRange(
        [
            "-f", "segment",
            "-segment_time", request.SegmentDurationSeconds.ToString(CultureInfo.InvariantCulture),
            "-segment_format", format.MuxerName
        ]);

        // MP4 writes the moov atom at the end by default, so a client that starts reading a segment before it is fully fetched would stall. The segments
        // are written as files, so the muxer can move the atom to the front of each one.
        if (string.Equals(format.MuxerName, "mp4", StringComparison.OrdinalIgnoreCase))
        {
            arguments.Add("-segment_format_options");
            arguments.Add("movflags=+faststart");
        }

        arguments.AddRange(
        [
            "-segment_start_number", "0",
            "-reset_timestamps", "1",
            "-segment_list", segmentListPath,
            "-segment_list_type", "csv",
            Path.Combine(temporaryPath, SEGMENT_FILE_NAME_PATTERN + format.FileExtension)
        ]);
        return arguments;
    }

    /// <summary>
    /// Parses the segment list ffmpeg wrote and turns it into the persisted index of the normalized audio.
    /// </summary>
    /// <remarks>
    /// Every line must describe a complete, readable segment. A malformed line is a hard failure instead of a skipped segment, because skipping a line
    /// would produce an index over audio that has a hole in it. The index of a segment is taken from the number in its generated file name, and the
    /// numbers must form a contiguous sequence starting at zero, so a missing segment can never be silently renumbered. A trailing segment with an empty
    /// time range, which ffmpeg can emit when the last frame ends exactly on a segment boundary, is discarded instead of being indexed. The produced
    /// duration is compared with the probed duration only for a shortfall, which is the truncation case; a production that is longer than the probe is
    /// kept and logged, because a probe that estimates a duration from the bitrate can underestimate a variable bitrate file. The duration recorded in the
    /// index is the produced end, rounded up to the next second, so the seek map of the cache entry always covers every produced segment.
    /// </remarks>
    /// <param name="cacheId">The opaque identifier of the cache entry.</param>
    /// <param name="segmentListPath">The file into which ffmpeg wrote the list of produced segments.</param>
    /// <param name="temporaryPath">The temporary directory the normalized audio was written into.</param>
    /// <param name="request">The track, the delivery decision and the quality cap the normalized audio was produced with.</param>
    /// <param name="format">The container and codec combination the normalized audio was produced in.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the index of the produced segments, or an error message.
    /// </returns>
    private Result<NormalizedTrackIndexDto> ParseSegmentIndex(string cacheId, string segmentListPath, string temporaryPath, NormalizedTrackRequestDto request, NormalizedAudioFormatDto format)
    {
        if (!File.Exists(segmentListPath))
            return Errors.Playback.NormalizationFailed;
        if (new FileInfo(segmentListPath).Length > MAX_SEGMENT_LIST_BYTES)
            return Errors.Playback.NormalizationFailed;

        List<NormalizedTrackSegmentIndexDto> segments = [];
        try
        {
            foreach (string line in File.ReadLines(segmentListPath))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // A csv entry is "file,start,end". The file name is generated by this application and never contains a comma, so the last two commas always delimit the two times.
                int endSeparatorIndex = line.LastIndexOf(',');
                if (endSeparatorIndex <= 0)
                    return Errors.Playback.NormalizationFailed;
                int startSeparatorIndex = line.LastIndexOf(',', endSeparatorIndex - 1);
                if (startSeparatorIndex <= 0)
                    return Errors.Playback.NormalizationFailed;

                string fileName = Path.GetFileName(line[..startSeparatorIndex].Trim());
                if (!TryGetSegmentNumber(fileName, format.FileExtension, out int segmentNumber))
                    return Errors.Playback.NormalizationFailed;

                string startText = line[(startSeparatorIndex + 1)..endSeparatorIndex].Trim();
                string endText = line[(endSeparatorIndex + 1)..].Trim();
                if (!decimal.TryParse(startText, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal startSeconds))
                    return Errors.Playback.NormalizationFailed;
                if (!decimal.TryParse(endText, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal endSeconds))
                    return Errors.Playback.NormalizationFailed;
                if (startSeconds < 0 || endSeconds < startSeconds)
                    return Errors.Playback.NormalizationFailed;

                FileInfo segmentFile = new(Path.Combine(temporaryPath, fileName));
                decimal durationSeconds = endSeconds - startSeconds;
                // ffmpeg can append a final segment whose time range is empty when the last frame ends exactly on a segment boundary. That segment is
                // discarded after the list is sorted, but a zero duration anywhere else, or a missing or empty segment that is supposed to carry audio, is a malformed production.
                if (durationSeconds == 0)
                {
                    segments.Add(new NormalizedTrackSegmentIndexDto(segmentNumber, startSeconds, 0, fileName, segmentFile.Exists ? segmentFile.Length : 0));
                    continue;
                }
                if (!segmentFile.Exists || segmentFile.Length <= 0)
                    return Errors.Playback.NormalizationFailed;

                segments.Add(new NormalizedTrackSegmentIndexDto(segmentNumber, startSeconds, durationSeconds, fileName, segmentFile.Length));
            }
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return Errors.Playback.NormalizationFailed;
        }

        if (segments.Count == 0 || segments.Count > MAX_SEGMENT_COUNT)
            return Errors.Playback.NormalizationFailed;

        // The number in the file name is the index, not the position of the line, and the numbers must be exactly the sequence from zero to count - 1.
        // A gap, a duplicate or an out of order line would otherwise produce an index over audio that the player would map to the wrong time.
        segments.Sort((left, right) => left.Index.CompareTo(right.Index));
        // The trailing segment with an empty time range carries no audio, so it is removed from the index and from the cache entry instead of being
        // exposed as a segment that the player would seek to and find empty.
        while (segments.Count > 0 && segments[^1].DurationSeconds == 0)
        {
            string trailingSegmentPath = Path.Combine(temporaryPath, segments[^1].FileName);
            segments.RemoveAt(segments.Count - 1);
            TryDeleteFile(trailingSegmentPath);
        }
        if (segments.Count == 0)
            return Errors.Playback.NormalizationFailed;

        decimal previousEndSeconds = 0;
        for (int position = 0; position < segments.Count; position++)
        {
            // A segment must start where the previous one ended, within the small tolerance of the csv times. Checking only that it does not start
            // before the previous end would accept a leading gap or a gap in the middle of the track as long as the final end still matched.
            decimal startDifferenceSeconds = segments[position].StartSeconds - previousEndSeconds;
            if (segments[position].Index != position ||
                segments[position].DurationSeconds <= 0 ||
                startDifferenceSeconds < -SEGMENT_TIME_TOLERANCE_SECONDS ||
                startDifferenceSeconds > SEGMENT_TIME_TOLERANCE_SECONDS)
                return Errors.Playback.NormalizationFailed;
            previousEndSeconds = segments[position].StartSeconds + segments[position].DurationSeconds;
        }

        // A shortfall is the truncation case this check exists for. A probe that estimates the duration from the bitrate can underestimate a variable
        // bitrate file, so a production that is longer than the probe is accepted and logged instead of being rejected forever.
        decimal expectedDurationSeconds = request.DurationInSeconds;
        decimal durationToleranceSeconds = Math.Max(DURATION_TOLERANCE_MIN_SECONDS, expectedDurationSeconds * DURATION_TOLERANCE_RATIO);
        if (previousEndSeconds < expectedDurationSeconds - durationToleranceSeconds)
            return Errors.Playback.NormalizationFailed;
        if (previousEndSeconds > expectedDurationSeconds + durationToleranceSeconds)
            _logger.LogInformation("The normalized audio of track {TrackId} in cache entry {CacheId} is longer than the probed duration: produced {ProducedSeconds} seconds, probed {ExpectedSeconds} seconds. The production is kept because only a shortfall is treated as truncation.", request.TrackId, cacheId, previousEndSeconds, expectedDurationSeconds);

        // The index advertises the produced end, rounded up to the next second, instead of the probed duration. A probe can underestimate a variable
        // bitrate file, and a seek map that is shorter than the audio would hide the tail of every segment after the underestimated end.
        int producedDurationSeconds = previousEndSeconds >= int.MaxValue
            ? int.MaxValue
            : (int)Math.Ceiling(previousEndSeconds);
        return new NormalizedTrackIndexDto(CACHE_INDEX_VERSION, request.TrackId, request.Mode, format.Container, format.Codec, request.SegmentDurationSeconds, producedDurationSeconds, segments);
    }

    /// <summary>
    /// Reads the persisted index of a cached normalized track, and validates that it is complete and that every segment it names still exists.
    /// </summary>
    /// <param name="cacheId">The opaque identifier of the cache entry.</param>
    /// <param name="index">The index of the cached normalized track, when it could be read.</param>
    /// <returns><see langword="true"/> when a complete and consistent cache entry could be read, otherwise <see langword="false"/>.</returns>
    private bool TryReadIndex(string cacheId, out NormalizedTrackIndexDto? index)
    {
        index = null;
        if (!TranscodingCacheCoordinator.IsValidCacheId(cacheId))
            return false;

        if (_cache.TryGetIndex(cacheId, out NormalizedTrackIndexDto? cachedIndex) && cachedIndex is not null)
        {
            index = cachedIndex;
            return true;
        }

        string cachePath = Path.Combine(_cacheRoot, cacheId);
        string indexPath = Path.Combine(cachePath, CACHE_INDEX_FILE_NAME);
        try
        {
            if (!File.Exists(indexPath))
                return false;
            if (new FileInfo(indexPath).Length > MAX_CACHE_INDEX_BYTES)
                return false;

            string indexJson = File.ReadAllText(indexPath);
            NormalizedTrackIndexDto? storedIndex = JsonSerializer.Deserialize<NormalizedTrackIndexDto>(indexJson, s_indexJsonOptions);
            if (storedIndex is null ||
                storedIndex.Version != CACHE_INDEX_VERSION ||
                storedIndex.TrackId == Guid.Empty ||
                storedIndex.Mode is not (PlaybackDeliveryMode.Remux or PlaybackDeliveryMode.Transcode) ||
                storedIndex.SegmentDurationSeconds <= 0 ||
                storedIndex.DurationInSeconds <= 0 ||
                storedIndex.Segments is null ||
                storedIndex.Segments.Count == 0 ||
                storedIndex.Segments.Count > MAX_SEGMENT_COUNT ||
                !PlaybackFormatCatalog.FormatsByPair.TryGetValue(PlaybackFormatCatalog.BuildPairKey(storedIndex.Container, storedIndex.Codec), out NormalizedAudioFormatDto? format))
                return false;

            decimal previousEndSeconds = 0;
            for (int position = 0; position < storedIndex.Segments.Count; position++)
            {
                NormalizedTrackSegmentIndexDto? segment = storedIndex.Segments[position];
                // The index must already be in playback order, name a supported generated file, describe a real segment and cover the track without a hole.
                decimal startDifferenceSeconds = segment is null ? 0 : segment.StartSeconds - previousEndSeconds;
                if (segment is null ||
                    segment.Index != position ||
                    !IsValidSegmentFileName(segment.FileName) ||
                    !TryGetSegmentNumber(segment.FileName, format.FileExtension, out int segmentNumber) ||
                    segmentNumber != position ||
                    segment.StartSeconds < 0 ||
                    segment.DurationSeconds <= 0 ||
                    segment.Length <= 0 ||
                    startDifferenceSeconds < -SEGMENT_TIME_TOLERANCE_SECONDS ||
                    startDifferenceSeconds > SEGMENT_TIME_TOLERANCE_SECONDS ||
                    !File.Exists(Path.Combine(cachePath, segment.FileName)))
                    return false;
                previousEndSeconds = segment.StartSeconds + segment.DurationSeconds;
            }

            // The advertised duration must cover the produced segments, otherwise a seek map built from the index would end before the audio does. New
            // indexes are written with the produced end rounded up; an index that claims less is treated as a corrupt cache entry.
            if (storedIndex.DurationInSeconds < previousEndSeconds)
                return false;

            index = storedIndex;
            _cache.SetIndex(cacheId, storedIndex);
            return true;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException or NotSupportedException)
        {
            return false;
        }
    }

    /// <summary>
    /// Reads a cached normalized track, when a complete cache entry for it exists.
    /// </summary>
    /// <param name="cacheId">The opaque identifier of the cache entry.</param>
    /// <param name="normalizedTrack">The cached normalized track, when it could be read.</param>
    /// <returns><see langword="true"/> when a complete cache entry could be read, otherwise <see langword="false"/>.</returns>
    private bool TryReadCachedTrack(string cacheId, out NormalizedTrackDto? normalizedTrack)
    {
        normalizedTrack = null;
        if (!TryReadIndex(cacheId, out NormalizedTrackIndexDto? index) || index is null)
            return false;

        normalizedTrack = GetOrCreateTrackProjection(cacheId, index);
        return true;
    }

    /// <summary>
    /// Projects a validated cache index once and reuses the projection for the following segment requests of the same cached track.
    /// </summary>
    /// <param name="cacheId">The opaque identifier of the cache entry that holds the normalized audio.</param>
    /// <param name="index">The validated index of the cache entry.</param>
    /// <returns>The normalized track described by the index.</returns>
    private NormalizedTrackDto GetOrCreateTrackProjection(string cacheId, NormalizedTrackIndexDto index)
    {
        (string CacheRoot, string CacheId) key = (_cacheRoot, cacheId);
        if (_trackProjections.TryGetValue(key, out NormalizedTrackDto? existing) && existing.TrackId == index.TrackId)
            return existing;
        NormalizedTrackDto projection = CreateNormalizedTrackDto(cacheId, index);
        // The projection is metadata only, but unbounded retention is not useful: when the bound is reached, the cache is dropped and rebuilt lazily.
        if (_trackProjections.Count >= MAX_TRACK_PROJECTIONS)
            _trackProjections.Clear();
        _trackProjections[key] = projection;
        return projection;
    }

    /// <summary>
    /// Projects a validated cache index into the normalized track the caller receives.
    /// </summary>
    /// <param name="cacheId">The opaque identifier of the cache entry that holds the normalized audio.</param>
    /// <param name="index">The validated index of the cache entry.</param>
    /// <returns>The normalized track described by the index.</returns>
    private static NormalizedTrackDto CreateNormalizedTrackDto(string cacheId, NormalizedTrackIndexDto index)
    {
        return new NormalizedTrackDto(
            index.TrackId,
            cacheId,
            index.Mode,
            index.Container,
            index.Codec,
            index.SegmentDurationSeconds,
            index.DurationInSeconds,
            [.. index.Segments.Select(segment => new NormalizedTrackSegmentDto(segment.Index, segment.StartSeconds, segment.DurationSeconds, segment.Length))]);
    }

    /// <summary>
    /// Builds the opaque cache identifier of a normalized track, so that a replaced or changed source file, or another track, never reuses a stale entry.
    /// </summary>
    /// <param name="request">The track, the delivery decision and the quality cap the normalized audio is produced with.</param>
    /// <param name="sourcePath">The full path of the stored audio file of the track.</param>
    /// <param name="sourceLength">The length of the stored audio file of the track, in bytes.</param>
    /// <param name="sourceLastModifiedUtc">The date and time, in UTC, when the stored audio file of the track was last modified.</param>
    /// <param name="format">The container and codec combination the normalized audio is produced in.</param>
    /// <returns>The opaque cache identifier.</returns>
    private static string BuildCacheId(NormalizedTrackRequestDto request, string sourcePath, long sourceLength, DateTime sourceLastModifiedUtc, NormalizedAudioFormatDto format)
    {
        string identity = string.Join('\n',
        [
            request.TrackId.ToString(),
            sourcePath,
            sourceLength.ToString(CultureInfo.InvariantCulture),
            sourceLastModifiedUtc.Ticks.ToString(CultureInfo.InvariantCulture),
            request.Mode.ToString(),
            format.Container,
            format.Codec,
            request.SegmentDurationSeconds.ToString(CultureInfo.InvariantCulture),
            request.DurationInSeconds.ToString(CultureInfo.InvariantCulture),
            // The quality cap only changes the output of a lossy transcode. A remux only repackages the stored codec, and a lossless transcode is not
            // capped, so including the quality for them would only create byte-identical entries that compete for the same cache budget.
            request.Mode == PlaybackDeliveryMode.Transcode && !format.IsLossless ? request.Quality.ToString() : string.Empty
        ]);
        // The identity is hashed so the cache entry name is a fixed length file name even when the source path is long, and so the source path is not
        // exposed by the directory listing of the cache.
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(identity))).ToLowerInvariant();
    }

    /// <summary>
    /// Resolves the ffmpeg demuxer of a stored container, so that the input is never probed by content.
    /// </summary>
    /// <param name="sourceContainer">The container of the stored audio file, as captured by the probe.</param>
    /// <param name="sourceDemuxer">The ffmpeg demuxer of the container, when it is known.</param>
    /// <returns><see langword="true"/> when the container maps to a known demuxer, otherwise <see langword="false"/>.</returns>
    private static bool TryResolveSourceDemuxer(string? sourceContainer, out string sourceDemuxer)
    {
        sourceDemuxer = string.Empty;
        if (string.IsNullOrWhiteSpace(sourceContainer))
            return false;
        if (!s_sourceDemuxers.TryGetValue(sourceContainer.Trim(), out string? demuxer) || string.IsNullOrEmpty(demuxer))
            return false;
        sourceDemuxer = demuxer;
        return true;
    }

    /// <summary>
    /// Gets the bitrate cap, in kbps, of a streaming quality setting.
    /// </summary>
    /// <param name="quality">The streaming quality cap of the current user.</param>
    /// <returns>The bitrate cap in kbps. <see cref="PlaybackStreamingQuality.Auto"/> uses the server default cap.</returns>
    private static int GetTargetBitrateKbps(PlaybackStreamingQuality quality)
    {
        return quality switch
        {
            PlaybackStreamingQuality.Low => 96,
            PlaybackStreamingQuality.Medium => 160,
            PlaybackStreamingQuality.High => 320,
            _ => AUTO_TRANSCODE_BITRATE_KBPS
        };
    }

    /// <summary>
    /// Estimates the size of the normalized audio, so that the cache is trimmed before a job starts instead of only after it finished.
    /// </summary>
    /// <remarks>
    /// A lossless delivery is estimated from the sample rate, the channel count and the bit depth of the stored audio, because a hi-res track can be
    /// several times larger than the uncompressed stereo audio at 44.1 kHz and 16 bits. A FLAC stream is compressed, so its initial estimate is a fraction
    /// of the uncompressed size. A remux of a lossy codec is estimated from the stored audio bitrate, not from the length of the source container, so a
    /// large video file with a small audio stream is not rejected as too big for the cache. The cache monitor grows the reservation when the actual output needs more room.
    /// </remarks>
    /// <param name="request">The delivery decision and the quality cap the normalized audio is produced with.</param>
    /// <param name="format">The container and codec combination the normalized audio is produced in.</param>
    /// <returns>The estimated size of the normalized audio, in bytes.</returns>
    private static long EstimateOutputSize(NormalizedTrackRequestDto request, NormalizedAudioFormatDto format)
    {
        if (format.IsLossless)
        {
            int sampleRate = request.SampleRate > 0 ? request.SampleRate : DEFAULT_SAMPLE_RATE;
            int channelCount = request.Channels > 0 ? request.Channels : DEFAULT_CHANNEL_COUNT;
            // The PCM encoder of the pipeline writes 16 bit samples, while the FLAC encoder keeps the sample format of the source, so the delivered
            // bit depth is used instead of the stored one for PCM. The FLAC estimate is a fraction of the uncompressed size, because the stream is compressed.
            int bitDepth = string.Equals(format.Container, "WAV", StringComparison.OrdinalIgnoreCase)
                ? DEFAULT_BIT_DEPTH
                : request.BitDepth is > 0 ? request.BitDepth.Value : DEFAULT_BIT_DEPTH;
            long uncompressedBytesPerSecond = (long)sampleRate * channelCount * bitDepth / 8;
            decimal compressionRatio = string.Equals(format.Container, "FLAC", StringComparison.OrdinalIgnoreCase) ? FLAC_COMPRESSION_RATIO : 1m;
            return Math.Max((long)(uncompressedBytesPerSecond * request.DurationInSeconds * compressionRatio), 1);
        }

        if (request.Mode == PlaybackDeliveryMode.Remux)
        {
            int sourceBitrateKbps = request.Bitrate is > 0 ? request.Bitrate.Value : DEFAULT_SOURCE_BITRATE_KBPS;
            decimal estimatedBytes = request.DurationInSeconds * sourceBitrateKbps * 1000m / 8m * REMUX_SIZE_MARGIN;
            return Math.Max((long)estimatedBytes, 1);
        }

        int bitrateKbps = GetTargetBitrateKbps(request.Quality);
        return Math.Max((long)request.DurationInSeconds * bitrateKbps * 1000 / 8, 1);
    }

    /// <summary>
    /// Checks whether a generated segment file name has the expected prefix and extension, and extracts the segment number from it.
    /// </summary>
    /// <param name="fileName">The file name of the segment.</param>
    /// <param name="fileExtension">The expected file extension of the segment, including the leading dot.</param>
    /// <param name="segmentNumber">The number encoded in the file name, when the name is valid.</param>
    /// <returns><see langword="true"/> when the file name is a valid generated segment name, otherwise <see langword="false"/>.</returns>
    private static bool TryGetSegmentNumber(string fileName, string fileExtension, out int segmentNumber)
    {
        segmentNumber = -1;
        if (string.IsNullOrEmpty(fileName) ||
            !fileName.StartsWith(SEGMENT_FILE_PREFIX, StringComparison.Ordinal) ||
            !fileName.EndsWith(fileExtension, StringComparison.Ordinal))
            return false;

        string numberText = fileName[SEGMENT_FILE_PREFIX.Length..^fileExtension.Length];
        return numberText.Length > 0 &&
               int.TryParse(numberText, NumberStyles.None, CultureInfo.InvariantCulture, out segmentNumber) &&
               segmentNumber >= 0;
    }

    /// <summary>
    /// Checks whether a value is a plain file name of a produced segment.
    /// </summary>
    /// <param name="fileName">The value to check.</param>
    /// <returns><see langword="true"/> when the value is a plain segment file name, otherwise <see langword="false"/>.</returns>
    private static bool IsValidSegmentFileName(string fileName)
    {
        return !string.IsNullOrWhiteSpace(fileName) &&
               string.Equals(fileName, Path.GetFileName(fileName), StringComparison.Ordinal) &&
               !fileName.Contains(Path.DirectorySeparatorChar) &&
               !fileName.Contains(Path.AltDirectorySeparatorChar);
    }

    /// <summary>
    /// Monitors the size of the temporary directory of a running job and keeps its cache reservation in step with the bytes it has actually written.
    /// </summary>
    /// <param name="cacheId">The opaque identifier of the cache entry.</param>
    /// <param name="trackId">The Id of the track whose audio is normalized.</param>
    /// <param name="outputSizeTracker">The tracker of the output size of the job.</param>
    /// <param name="reservation">The capacity the job reserved in the cache.</param>
    /// <param name="process">The ffmpeg process of the job.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns><see langword="true"/> when the job was stopped because it could not reserve enough cache capacity, otherwise <see langword="false"/>.</returns>
    private async Task<bool> MonitorOutputSizeAsync(string cacheId, Guid trackId, TranscodingOutputSizeTracker outputSizeTracker, TranscodingCacheReservation reservation, Process process, CancellationToken cancellationToken)
    {
        try
        {
            while (true)
            {
                await Task.Delay(CACHE_SIZE_MONITOR_INTERVAL_MILLISECONDS, cancellationToken).ConfigureAwait(false);
                long actualBytes = outputSizeTracker.GetCurrentSize();
                if (!reservation.TryUpdate(actualBytes, _settings.MaxCacheBytes, _evictionGracePeriod))
                {
                    _logger.LogWarning("A normalization job for track {TrackId} in cache entry {CacheId} could not reserve enough space in the normalized audio cache and was stopped.", trackId, cacheId);
                    KillProcess(process);
                    return true;
                }
            }
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }

    /// <summary>
    /// Waits for the bounded standard error of a finished process, without pinning the job when the pipe never closes.
    /// </summary>
    /// <param name="standardErrorTask">The task that reads the bounded standard error of the process.</param>
    /// <returns>The bounded standard error, or an empty string when the pipe did not close within the bounded wait.</returns>
    private static async Task<string> AwaitStandardErrorAsync(Task<string> standardErrorTask)
    {
        try
        {
            return await standardErrorTask.WaitAsync(TimeSpan.FromSeconds(PROCESS_EXIT_TIMEOUT_SECONDS)).ConfigureAwait(false);
        }
        catch (TimeoutException)
        {
            // A kill that did not take effect leaves the pipe open. The job is finished without a diagnostic instead of holding a job slot forever; the
            // read task ends when the process is disposed and its standard error stream is closed.
            return string.Empty;
        }
    }

    /// <summary>
    /// Reads the standard error of a running process while keeping only its last few kilobytes, so a crafted input cannot make the server buffer an unbounded diagnostic string over the whole job timeout.
    /// </summary>
    /// <param name="standardError">The standard error stream of the process.</param>
    /// <returns>The bounded tail of the standard error of the process.</returns>
    private static async Task<string> ReadBoundedStandardErrorAsync(StreamReader standardError)
    {
        char[] buffer = new char[4096];
        StringBuilder boundedStandardError = new();
        try
        {
            int read;
            while ((read = await standardError.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false)) > 0)
            {
                boundedStandardError.Append(buffer, 0, read);
                if (boundedStandardError.Length > MAX_STANDARD_ERROR_CHARACTERS * 2)
                    boundedStandardError.Remove(0, boundedStandardError.Length - MAX_STANDARD_ERROR_CHARACTERS);
            }
        }
        catch (Exception exception) when (exception is IOException or ObjectDisposedException)
        {
            // The pipe is closed when the process is killed; the bytes read so far are still the useful diagnostic tail.
        }

        if (boundedStandardError.Length > MAX_STANDARD_ERROR_CHARACTERS)
            boundedStandardError.Remove(0, boundedStandardError.Length - MAX_STANDARD_ERROR_CHARACTERS);
        return boundedStandardError.ToString();
    }

    /// <summary>
    /// Replaces an absolute source path in a diagnostic message with a placeholder, so the path is not written to the application log.
    /// </summary>
    /// <param name="diagnostic">The diagnostic message.</param>
    /// <param name="path">The path to redact, or <see langword="null"/> when there is none.</param>
    /// <returns>The diagnostic message with the path replaced.</returns>
    private static string RedactPath(string diagnostic, string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return diagnostic;
        return diagnostic.Replace(path, "[source]", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Removes a completed production from the shared registry, unless a newer production replaced it.
    /// </summary>
    /// <param name="normalizationJobKey">The cache root and cache entry the production belongs to.</param>
    /// <param name="normalizationJob">The production that completed.</param>
    private static void RemoveNormalizationJob((string CacheRoot, string CacheId) normalizationJobKey, Lazy<Task<Result<NormalizedTrackDto>>> normalizationJob)
    {
        if (s_normalizationJobs.TryGetValue(normalizationJobKey, out Lazy<Task<Result<NormalizedTrackDto>>>? currentJob) && ReferenceEquals(currentJob, normalizationJob))
            s_normalizationJobs.TryRemove(normalizationJobKey, out _);
    }

    /// <summary>
    /// Deletes a file, ignoring a file that is already gone or cannot be deleted.
    /// </summary>
    /// <param name="filePath">The file to delete.</param>
    private static void TryDeleteFile(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            // The trailing empty segment is not indexed, so a failed deletion only leaves a small unreferenced file in the cache entry.
        }
    }

    /// <summary>
    /// Stops a child process and its descendants without waiting for it, so a cancellation callback is never blocked; the caller that awaits the process observes its exit.
    /// </summary>
    /// <param name="process">The process to stop.</param>
    private void KillProcess(Process process)
    {
        try
        {
            if (!process.HasExited)
                process.Kill(entireProcessTree: true);
        }
        catch (Exception exception) when (exception is InvalidOperationException or NotSupportedException or Win32Exception)
        {
            _logger.LogDebug(exception, "The audio transcoder process could not be stopped.");
        }
    }

    /// <summary>
    /// Stops every child process that is still running when the application exits, and cancels the productions that are still waiting for a job slot.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="eventArguments">The event data.</param>
    private void OnApplicationStopping(object? sender, EventArgs eventArguments)
    {
        _shutdownCancellation.Cancel();
        foreach (Process process in _runningProcesses.Values)
            KillProcess(process);
    }

    /// <summary>
    /// Releases the resources of the service when its host is disposed, including the process exit subscription that would otherwise keep it alive.
    /// </summary>
    /// <remarks>
    /// The cancellation source, the job slots and the cache coordinator are process lifetime resources that a production uses until it leaves. Shutdown
    /// cancels and kills first, then waits a bounded time for every admitted production to leave before disposing them, so a production that is finishing
    /// during shutdown still reports its Result instead of faulting on a disposed primitive or re-registering into a released coordinator. If the drain
    /// times out the resources are intentionally left undisposed: the process is exiting and faulting the in-flight task would be worse.
    /// </remarks>
    public void Dispose()
    {
        lock (_lifecycleSync)
        {
            if (_isDisposed)
                return;
            _isDisposed = true;
        }
        AppDomain.CurrentDomain.ProcessExit -= OnApplicationStopping;
        OnApplicationStopping(this, EventArgs.Empty);
        if (!_productionsDrained.Wait(TimeSpan.FromSeconds(PRODUCTION_DRAIN_TIMEOUT_SECONDS)))
        {
            _logger.LogWarning("The normalized audio productions did not drain within {DrainTimeoutSeconds} seconds during shutdown. The process lifetime resources are left undisposed.", PRODUCTION_DRAIN_TIMEOUT_SECONDS);
            return;
        }
        _cache.Release();
        _jobSlots.Dispose();
        _shutdownCancellation.Dispose();
        _productionsDrained.Dispose();
    }
}
