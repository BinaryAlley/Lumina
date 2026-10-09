#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.PathTemplate;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Services.Jobs;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Jobs.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Jobs.Audio.Music;

/// <summary>
/// Media library scan job for extracting the metadata of the discovered music file system items.
/// </summary>
internal sealed class MusicMetadataExtractionJob : MediaLibraryScanJob, IMusicMetadataExtractionJob
{
    private const int EXTRACTION_BATCH_SIZE = 1000; // The number of extracted metadata items that are written to the staging results in a single batch, keeping the peak memory bounded regardless of the library size.
    private static readonly TagLib.TagTypes[] s_extendedTagContainerTypes = [TagLib.TagTypes.Xiph, TagLib.TagTypes.Id3v2, TagLib.TagTypes.Apple, TagLib.TagTypes.Ape];
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<MusicMetadataExtractionJob> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MusicMetadataExtractionJob"/> class.
    /// </summary>
    /// <param name="serviceScopeFactory">
    /// Injected factory for creating scopes in which services are requested.
    /// See docs/technical/architecture/architecture-knowledge-management/architecture-decision-log/architecture-decision-record-0001.md for details.
    /// </param>
    /// <param name="logger">Injected service for logging.</param>
    public MusicMetadataExtractionJob(IServiceScopeFactory serviceScopeFactory, ILogger<MusicMetadataExtractionJob> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    /// <summary>
    /// Executes the payload of the media library scan job.
    /// </summary>
    /// <typeparam name="TInput">The type of the input parameter representing the data to be processed by this payload.</typeparam>
    /// <param name="id">The id of the media library scan job.</param>
    /// <param name="input">The input data to be processed by this payload.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task ExecuteAsync<TInput>(Guid id, TInput input, CancellationToken cancellationToken)
    {
        try
        {
            // Increment the number of parents that finished their execution and called this job (beware race conditions, jobs run in parallel).
            int parentsCompleted = Interlocked.Increment(ref parentsPayloadsExecuted);
            // Only execute this job's payload when it has no parents, or when all the parents finished their execution.
            if (Parents.Count == 0 || parentsCompleted == Parents.Count)
            {
                // This needs to be wrapped in a task because even though this job is processed in a "fire and forget" async manner, it still does synchronous
                // file system processing that takes time, and would block the processing of scan jobs in the in-memory queue.
                await Task.Run(async () =>
                {
                    Status = LibraryScanJobStatus.Running;
                    // See docs/technical/architecture/architecture-knowledge-management/architecture-decision-log/architecture-decision-record-0001.md for details.
                    await using AsyncServiceScope asyncServiceScope = _serviceScopeFactory.CreateAsyncScope();
                    IUnitOfWork unitOfWork = asyncServiceScope.ServiceProvider.GetService<IUnitOfWork>()!;
                    IDomainEventPublisher domainEventPublisher = asyncServiceScope.ServiceProvider.GetService<IDomainEventPublisher>()!;

                    MediaLibraryScanCompositeId compositeKey = MediaLibraryScanCompositeId.Create(ScanId, UserId);

                    // Get the library from the repository, both to verify that it exists and to know its content locations.
                    Result<LibraryEntity?> getLibraryResult = await unitOfWork.LibraryRepository.GetByIdAsync(LibraryId.Value, cancellationToken: cancellationToken).ConfigureAwait(false);
                    if (getLibraryResult.IsFailure || getLibraryResult.Value is null)
                        throw new InvalidOperationException(getLibraryResult.IsFailure ? getLibraryResult.FirstError.Description : "The media library was not found.");
                    Result<Library> domainLibraryResult = getLibraryResult.Value.ToDomainEntity();
                    if (domainLibraryResult.IsFailure)
                        throw new InvalidOperationException(domainLibraryResult.FirstError.Description);

                    // The library always carries a path template: the one configured by the user, or the ideal structure of the library type as its default.
                    ILibraryPathTemplateService pathTemplateService = asyncServiceScope.ServiceProvider.GetService<ILibraryPathTemplateService>()!;
                    IPathService pathService = asyncServiceScope.ServiceProvider.GetService<IPathService>()!;
                    Result<LibraryPathTemplate> pathTemplateResult = pathTemplateService.ResolveTemplate(domainLibraryResult.Value.LibraryType, domainLibraryResult.Value.PathTemplate.Parts);
                    if (pathTemplateResult.IsFailure)
                        throw new InvalidOperationException(pathTemplateResult.FirstError.Description);
                    LibraryPathTemplate pathTemplate = pathTemplateResult.Value;

                    // The metadata is extracted only for the files that are new or changed, since the unchanged ones already have their materialized items.
                    Result<IReadOnlyList<string>> getPathsResult = await unitOfWork.LibraryScanStagingResultsRepository.GetPathsNeedingRehashAsync(ScanId.Value, cancellationToken).ConfigureAwait(false);
                    if (getPathsResult.IsFailure)
                        throw new InvalidOperationException(getPathsResult.FirstError.Description);
                    IReadOnlyList<string> paths = getPathsResult.Value;

                    // Set the initial progress of the scan job.
                    Result<Success> publishJobProgressResult = await PublishJobProgressAsync(domainEventPublisher, compositeKey, 0, paths.Count, cancellationToken).ConfigureAwait(false);
                    if (publishJobProgressResult.IsFailure)
                        throw new InvalidOperationException(publishJobProgressResult.FirstError.Description);

                    TimeSpan heartbeatInterval = TimeSpan.FromSeconds(1);
                    DateTime lastHeartbeat = DateTime.UtcNow;
                    int processedItems = 0;

                    List<MusicLibraryScanItemMetadataEntity> metadataBatch = [];
                    foreach (string path in paths)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        metadataBatch.Add(ExtractMetadata(path, domainLibraryResult.Value.ContentLocations, domainLibraryResult.Value.LibraryType, pathTemplate, pathTemplateService, pathService));
                        if (metadataBatch.Count >= EXTRACTION_BATCH_SIZE)
                        {
                            Result<Created> insertBatchResult = await unitOfWork.MusicLibraryScanItemMetadataRepository.InsertRangeAsync(metadataBatch, cancellationToken).ConfigureAwait(false);
                            if (insertBatchResult.IsFailure)
                                throw new InvalidOperationException(insertBatchResult.FirstError.Description);
                            Result<Success> saveBatchResult = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                            if (saveBatchResult.IsFailure)
                                throw new InvalidOperationException(saveBatchResult.FirstError.Description);
                            metadataBatch.Clear();
                            unitOfWork.ClearTrackedEntities();
                        }
                        processedItems++;

                        // Increment the number of processed elements progress in a high-frequency counter with low overhead check.
                        DateTime now = DateTime.UtcNow;
                        if (now - lastHeartbeat >= heartbeatInterval)
                        {
                            publishJobProgressResult = await PublishJobProgressAsync(domainEventPublisher, compositeKey, processedItems, paths.Count, cancellationToken).ConfigureAwait(false);
                            if (publishJobProgressResult.IsFailure)
                                throw new InvalidOperationException(publishJobProgressResult.FirstError.Description);
                            lastHeartbeat = now;
                        }
                    }

                    // Flush any remaining extracted metadata items that did not reach the batch size.
                    if (metadataBatch.Count > 0)
                    {
                        Result<Created> insertBatchResult = await unitOfWork.MusicLibraryScanItemMetadataRepository.InsertRangeAsync(metadataBatch, cancellationToken).ConfigureAwait(false);
                        if (insertBatchResult.IsFailure)
                            throw new InvalidOperationException(insertBatchResult.FirstError.Description);
                        Result<Success> saveBatchResult = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                        if (saveBatchResult.IsFailure)
                            throw new InvalidOperationException(saveBatchResult.FirstError.Description);
                    }

                    // This job finished, increment the number of processed jobs progress.
                    await domainEventPublisher.PublishAsync(new LibraryScanProgressChangedDomainEvent(Guid.NewGuid(), LibraryId, compositeKey, DateTime.UtcNow), cancellationToken).ConfigureAwait(false);
                    Status = LibraryScanJobStatus.Completed;

                    // Call each linked child with the obtained payload.
                    foreach (IMediaLibraryScanJob child in Children)
                        await child.ExecuteAsync(id, input, cancellationToken).ConfigureAwait(false);
                }, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            Status = LibraryScanJobStatus.Canceled;
            throw;
        }
        catch (Exception exception)
        {
            Status = LibraryScanJobStatus.Failed;
            await ScanFailurePublisher.PublishAsync(_serviceScopeFactory, LibraryId, MediaLibraryScanCompositeId.Create(ScanId, UserId), exception, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Extracts the metadata of the music file stored at <paramref name="path"/>.
    /// </summary>
    /// <param name="path">The file system path of the music file whose metadata is extracted.</param>
    /// <param name="contentLocations">The content locations of the media library the music file belongs to.</param>
    /// <param name="libraryType">The type of the media library the music file belongs to.</param>
    /// <param name="pathTemplate">The template describing the structure of the media library on disk.</param>
    /// <param name="pathTemplateService">The service used to derive the metadata from the path of the music file.</param>
    /// <param name="pathService">The service used to determine the path separator and whether the file is inside the content locations of the media library.</param>
    /// <returns>The staged music metadata item.</returns>
    private MusicLibraryScanItemMetadataEntity ExtractMetadata(string path, IReadOnlyCollection<FileSystemPathId> contentLocations, LibraryType libraryType, LibraryPathTemplate pathTemplate, ILibraryPathTemplateService pathTemplateService, IPathService pathService)
    {
        string relativePath = GetRelativePath(path, contentLocations, pathService);
        Result<Optional<ParsedLibraryPath>> pathMetadataResult = pathTemplateService.Parse(libraryType, pathTemplate, relativePath, pathService.PathSeparator);

        string? artistName = null;
        MusicReleaseType? releaseType = null;
        int? releaseYear = null;
        string? releaseName = null;
        int? trackNumber = null;
        int? discNumber = null;
        string? trackTitle = null;
        Guid? musicBrainzArtistId = null;
        Guid? musicBrainzReleaseArtistId = null;
        Guid? musicBrainzReleaseGroupId = null;
        Guid? musicBrainzReleaseId = null;
        Guid? musicBrainzRecordingId = null;
        Guid? musicBrainzTrackId = null;
        Guid? musicBrainzWorkId = null;
        string? workTitle = null;

        if (pathMetadataResult.IsSuccess && pathMetadataResult.Value.HasValue)
        {
            MusicLibraryPathMetadata pathMetadata = MusicLibraryPathMetadata.FromParsedLibraryPath(pathMetadataResult.Value.Value);
            if (pathMetadata.ArtistName.HasValue)
                artistName = pathMetadata.ArtistName.Value;
            if (pathMetadata.ReleaseType.HasValue)
                releaseType = pathMetadata.ReleaseType.Value;
            if (pathMetadata.ReleaseYear.HasValue)
                releaseYear = pathMetadata.ReleaseYear.Value;
            if (pathMetadata.ReleaseName.HasValue)
                releaseName = pathMetadata.ReleaseName.Value;
            if (pathMetadata.TrackNumber.HasValue)
                trackNumber = pathMetadata.TrackNumber.Value;
            if (pathMetadata.DiscNumber.HasValue)
                discNumber = pathMetadata.DiscNumber.Value;
            if (pathMetadata.TrackTitle.HasValue)
                trackTitle = pathMetadata.TrackTitle.Value;
        }

        int durationInSeconds = 0;
        int sampleRate = 0;
        int channels = 0;
        int? bitDepth = null;
        string? audioCodec = null;
        int? bitrate = null;
        string? acoustId = null;
        decimal? replayGainTrackGain = null;
        decimal? replayGainTrackPeak = null;
        decimal? replayGainAlbumGain = null;
        decimal? replayGainAlbumPeak = null;
        List<string> moods = [];

        try
        {
            using (TagLib.File tagFile = TagLib.File.Create(path))
            {
                TagLib.Tag tag = tagFile.Tag;
                if (!string.IsNullOrWhiteSpace(tag.Title))
                    trackTitle = tag.Title;
                if (!string.IsNullOrWhiteSpace(tag.Album))
                    releaseName = tag.Album;
                // The album artist is preferred over the track artist, because it is the artist the release belongs to.
                string? tagArtist = !string.IsNullOrWhiteSpace(tag.FirstAlbumArtist) ? tag.FirstAlbumArtist : tag.FirstPerformer;
                if (!string.IsNullOrWhiteSpace(tagArtist))
                    artistName = tagArtist;
                if (tag.Track > 0)
                    trackNumber = (int)tag.Track;
                if (tag.Disc > 0)
                    discNumber = (int)tag.Disc;
                if (tag.Year > 0)
                    releaseYear = (int)tag.Year;

                TagLib.Properties audioProperties = tagFile.Properties;
                durationInSeconds = (int)audioProperties.Duration.TotalSeconds;
                sampleRate = audioProperties.AudioSampleRate;
                channels = audioProperties.AudioChannels;
                if (audioProperties.BitsPerSample > 0)
                    bitDepth = audioProperties.BitsPerSample;
                if (audioProperties.AudioBitrate > 0)
                    bitrate = audioProperties.AudioBitrate;
                if (!string.IsNullOrWhiteSpace(audioProperties.Description))
                    audioCodec = audioProperties.Description;

                // The text frames of the ID3v2 tag are read once for the whole file, so that the extended tag lookups below do not walk all the frames
                // of the tag again for every key.
                IReadOnlyDictionary<string, string[]> id3v2ExtendedFields = GetId3v2ExtendedFields(tagFile);

                // The MusicBrainz identifiers written by taggers like Picard are read here, so that the enrichment can look the items up directly,
                // instead of guessing them through searches. The work identifier and title are not exposed as properties by the tagging library, so
                // they are read from the extended tags of the container, and they only serve as a fallback for recordings that MusicBrainz leaves unlinked to a work.
                musicBrainzArtistId = ParseGuid(tag.MusicBrainzArtistId);
                musicBrainzReleaseArtistId = ParseGuid(tag.MusicBrainzReleaseArtistId);
                musicBrainzReleaseGroupId = ParseGuid(tag.MusicBrainzReleaseGroupId);
                musicBrainzReleaseId = ParseGuid(tag.MusicBrainzReleaseId);
                musicBrainzRecordingId = ParseGuid(tag.MusicBrainzTrackId);
                musicBrainzTrackId = ParseGuid(GetExtendedField(tagFile, id3v2ExtendedFields, "MusicBrainz Release Track Id", "MUSICBRAINZ_RELEASETRACKID"));
                musicBrainzWorkId = ParseGuid(GetExtendedField(tagFile, id3v2ExtendedFields, "MusicBrainz Work Id", "MUSICBRAINZ_WORKID"));
                string? extendedWorkTitle = GetExtendedField(tagFile, id3v2ExtendedFields, "WORK", "MUSICBRAINZ_WORK");
                if (!string.IsNullOrWhiteSpace(extendedWorkTitle))
                    workTitle = extendedWorkTitle;

                // The AcoustID, the moods and the ReplayGain values written by taggers like Picard are read here as well. The moods can be a single
                // slash separated value or several distinct values, depending on the container, so all of them are read and split before being de-duplicated.
                acoustId = GetExtendedField(tagFile, id3v2ExtendedFields, "Acoustid Id", "ACOUSTID_ID");
                moods = [.. GetExtendedFields(tagFile, id3v2ExtendedFields, "mood", "MOOD", "MOODS")
                    .SelectMany(mood => mood.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                    .Where(mood => !string.IsNullOrWhiteSpace(mood))
                    .Distinct(StringComparer.OrdinalIgnoreCase)];
                replayGainTrackGain = ParseDecimal(GetExtendedField(tagFile, id3v2ExtendedFields, "REPLAYGAIN_TRACK_GAIN", "replaygain_track_gain"));
                replayGainTrackPeak = ParseDecimal(GetExtendedField(tagFile, id3v2ExtendedFields, "REPLAYGAIN_TRACK_PEAK", "replaygain_track_peak"));
                replayGainAlbumGain = ParseDecimal(GetExtendedField(tagFile, id3v2ExtendedFields, "REPLAYGAIN_ALBUM_GAIN", "replaygain_album_gain"));
                replayGainAlbumPeak = ParseDecimal(GetExtendedField(tagFile, id3v2ExtendedFields, "REPLAYGAIN_ALBUM_PEAK", "replaygain_album_peak"));
            }
        }
        catch (Exception exception) when (exception is TagLib.CorruptFileException or TagLib.UnsupportedFormatException or IOException or UnauthorizedAccessException)
        {
            // A music file whose tags cannot be read still contributes the metadata derived from its path, but the failure is logged, so that a
            // systematic tagging failure of a whole library does not silently degrade every file to path only metadata.
            _logger.LogWarning(exception, "Failed to read the embedded tags of the music file '{Path}'. Its metadata will be derived from the path only.", path);
        }

        if (string.IsNullOrWhiteSpace(trackTitle))
            trackTitle = Path.GetFileNameWithoutExtension(path);

        return new MusicLibraryScanItemMetadataEntity
        {
            Id = Guid.NewGuid(),
            LibraryScanId = ScanId.Value,
            LibraryId = LibraryId.Value,
            Path = path,
            ArtistName = artistName,
            ReleaseType = releaseType,
            ReleaseYear = releaseYear,
            ReleaseName = releaseName,
            TrackTitle = trackTitle,
            TrackNumber = trackNumber,
            DiscNumber = discNumber,
            DurationInSeconds = durationInSeconds,
            SampleRate = sampleRate,
            Channels = channels,
            BitDepth = bitDepth,
            AudioCodec = audioCodec,
            Bitrate = bitrate,
            AcoustId = acoustId,
            ReplayGainTrackGain = replayGainTrackGain,
            ReplayGainTrackPeak = replayGainTrackPeak,
            ReplayGainAlbumGain = replayGainAlbumGain,
            ReplayGainAlbumPeak = replayGainAlbumPeak,
            Moods = moods,
            MusicBrainzArtistId = musicBrainzArtistId,
            MusicBrainzReleaseArtistId = musicBrainzReleaseArtistId,
            MusicBrainzReleaseGroupId = musicBrainzReleaseGroupId,
            MusicBrainzReleaseId = musicBrainzReleaseId,
            MusicBrainzRecordingId = musicBrainzRecordingId,
            MusicBrainzTrackId = musicBrainzTrackId,
            MusicBrainzWorkId = musicBrainzWorkId,
            WorkTitle = workTitle
        };
    }

    /// <summary>
    /// Parses the provided tag <paramref name="value"/> as a globally unique identifier.
    /// </summary>
    /// <param name="value">The tag value to parse.</param>
    /// <returns>The parsed identifier, or <see langword="null"/> when the value is missing or malformed.</returns>
    private static Guid? ParseGuid(string? value)
    {
        return Guid.TryParse(value, out Guid parsedValue) ? parsedValue : null;
    }

    /// <summary>
    /// Reads the text frames of the ID3v2 tag of the provided <paramref name="tagFile"/> once, keyed by their description and compared
    /// case-insensitively, so that every extended tag lookup of the file reuses the same enumeration instead of walking all the frames again.
    /// </summary>
    /// <param name="tagFile">The file whose ID3v2 text frames are read.</param>
    /// <returns>The text values of the ID3v2 frames, keyed by their description, or an empty dictionary when the file has no ID3v2 tag.</returns>
    private static Dictionary<string, string[]> GetId3v2ExtendedFields(TagLib.File tagFile)
    {
        Dictionary<string, string[]> fields = new(StringComparer.OrdinalIgnoreCase);
        if (tagFile.GetTag(TagLib.TagTypes.Id3v2) is not TagLib.Id3v2.Tag id3v2Tag)
            return fields;

        // Only the first frame of a given description is kept, so a description that appears more than once always resolves to its first occurrence.
        foreach (TagLib.Id3v2.Frame frame in id3v2Tag.GetFrames())
            if (frame is TagLib.Id3v2.UserTextInformationFrame userTextViewFrame
                && !string.IsNullOrEmpty(userTextViewFrame.Description)
                && !fields.ContainsKey(userTextViewFrame.Description))
                fields[userTextViewFrame.Description] = userTextViewFrame.Text;
        return fields;
    }

    /// <summary>
    /// Reads an extended tag that the tagging library does not expose as a property, from whichever container tag of the file carries it.
    /// </summary>
    /// <param name="tagFile">The file whose extended tags are read.</param>
    /// <param name="id3v2ExtendedFields">The text frames of the ID3v2 tag of the file, read once for the whole file.</param>
    /// <param name="keys">The candidate names of the extended tag to read, in the order they are tried.</param>
    /// <returns>The first non-empty value found, or <see langword="null"/> when the file carries none of the provided tags.</returns>
    private static string? GetExtendedField(TagLib.File tagFile, IReadOnlyDictionary<string, string[]> id3v2ExtendedFields, params string[] keys)
    {
        foreach (TagLib.TagTypes tagType in s_extendedTagContainerTypes)
        {
            // The frames of the ID3v2 tag were read once for the whole file, so they are looked up instead of being enumerated again for every key.
            if (tagType == TagLib.TagTypes.Id3v2)
            {
                foreach (string key in keys)
                    if (id3v2ExtendedFields.TryGetValue(key, out string[]? id3v2Values) && id3v2Values.Length > 0 && !string.IsNullOrWhiteSpace(id3v2Values[0]))
                        return id3v2Values[0];
                continue;
            }

            TagLib.Tag? tag = tagFile.GetTag(tagType);
            if (tag is null)
                continue;

            foreach (string key in keys)
            {
                string? value = GetExtendedFieldFromTag(tag, key);
                if (!string.IsNullOrWhiteSpace(value))
                    return value;
            }
        }
        return null;
    }

    /// <summary>
    /// Reads an extended tag with the provided <paramref name="key"/> from the provided <paramref name="tag"/>, according to its container format.
    /// </summary>
    /// <param name="tag">The container tag whose extended tags are read.</param>
    /// <param name="key">The name of the extended tag to read.</param>
    /// <returns>The value of the extended tag, or <see langword="null"/> when the tag does not carry it.</returns>
    private static string? GetExtendedFieldFromTag(TagLib.Tag tag, string key)
    {
        switch (tag)
        {
            case TagLib.Ogg.XiphComment xiphComment:
                string[] xiphValues = xiphComment.GetField(key);
                return xiphValues.Length > 0 ? xiphValues[0] : null;
            case TagLib.Mpeg4.AppleTag appleTag:
                return appleTag.GetDashBox("com.apple.iTunes", key);
            case TagLib.Ape.Tag apeTag:
                if (apeTag.HasItem(key) && apeTag.GetItem(key) is TagLib.Ape.Item apeItem)
                {
                    string[] apeValues = apeItem.ToStringArray();
                    return apeValues.Length > 0 ? apeValues[0] : null;
                }
                return null;
            default:
                return null;
        }
    }

    /// <summary>
    /// Reads every value of an extended tag that the tagging library does not expose as a property, from whichever container tag of the file carries it.
    /// </summary>
    /// <param name="tagFile">The file whose extended tags are read.</param>
    /// <param name="id3v2ExtendedFields">The text frames of the ID3v2 tag of the file, read once for the whole file.</param>
    /// <param name="keys">The candidate names of the extended tag to read, in the order they are tried.</param>
    /// <returns>Every non-empty value found, or an empty list when the file carries none of the provided tags.</returns>
    private static List<string> GetExtendedFields(TagLib.File tagFile, IReadOnlyDictionary<string, string[]> id3v2ExtendedFields, params string[] keys)
    {
        List<string> values = [];
        foreach (TagLib.TagTypes tagType in s_extendedTagContainerTypes)
        {
            // The frames of the ID3v2 tag were read once for the whole file, so they are looked up instead of being enumerated again for every key.
            if (tagType == TagLib.TagTypes.Id3v2)
            {
                foreach (string key in keys)
                    if (id3v2ExtendedFields.TryGetValue(key, out string[]? id3v2Values))
                        values.AddRange(id3v2Values.Where(value => !string.IsNullOrWhiteSpace(value)));
                continue;
            }

            TagLib.Tag? tag = tagFile.GetTag(tagType);
            if (tag is null)
                continue;

            foreach (string key in keys)
                values.AddRange(GetExtendedFieldsFromTag(tag, key).Where(value => !string.IsNullOrWhiteSpace(value)));
        }
        return values;
    }

    /// <summary>
    /// Reads every value of an extended tag with the provided <paramref name="key"/> from the provided <paramref name="tag"/>, according to its container format.
    /// </summary>
    /// <param name="tag">The container tag whose extended tags are read.</param>
    /// <param name="key">The name of the extended tag to read.</param>
    /// <returns>Every value of the extended tag, or an empty list when the tag does not carry it.</returns>
    private static List<string> GetExtendedFieldsFromTag(TagLib.Tag tag, string key)
    {
        switch (tag)
        {
            case TagLib.Ogg.XiphComment xiphComment:
                return [.. xiphComment.GetField(key)];
            case TagLib.Mpeg4.AppleTag appleTag:
                string? value = appleTag.GetDashBox("com.apple.iTunes", key);
                return string.IsNullOrWhiteSpace(value) ? [] : [.. value.Split('\0', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)];
            case TagLib.Ape.Tag apeTag:
                if (apeTag.HasItem(key) && apeTag.GetItem(key) is TagLib.Ape.Item apeItem)
                    return [.. apeItem.ToStringArray()];
                return [];
            default:
                return [];
        }
    }

    /// <summary>
    /// Parses a ReplayGain value, optionally suffixed with its unit, into a decimal.
    /// </summary>
    /// <param name="value">The value to parse.</param>
    /// <returns>The parsed decimal, or <see langword="null"/> when the value is missing or malformed.</returns>
    private static decimal? ParseDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        string cleaned = value.Replace("dB", string.Empty, StringComparison.OrdinalIgnoreCase).Trim();
        return decimal.TryParse(cleaned, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal parsed) ? parsed : null;
    }

    /// <summary>
    /// Gets the path of the music file stored at <paramref name="path"/>, relative to the content location of the media library it belongs to.
    /// </summary>
    /// <param name="path">The absolute file system path of the music file.</param>
    /// <param name="contentLocations">The content locations of the media library the music file belongs to.</param>
    /// <param name="pathService">The service used to determine whether the file is inside a content location of the media library.</param>
    /// <returns>The path of the music file, relative to its content location, or the file name when no content location contains it.</returns>
    private static string GetRelativePath(string path, IReadOnlyCollection<FileSystemPathId> contentLocations, IPathService pathService)
    {
        foreach (FileSystemPathId contentLocation in contentLocations)
        {
            if (pathService.IsPathWithin(path, contentLocation.Path))
            {
                string relativePath = path[contentLocation.Path.Length..].TrimStart('\\', '/');
                if (relativePath.Length > 0)
                    return relativePath;
            }
        }
        return Path.GetFileName(path);
    }

    /// <summary>
    /// Publishes a job progress update.
    /// </summary>
    /// <param name="domainEventPublisher">The service used to publish the progress update.</param>
    /// <param name="compositeKey">The composite unique identifier of a media library scan.</param>
    /// <param name="currentProgress">The current job progress.</param>
    /// <param name="totalProgress">The total job progress.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    private async Task<Result<Success>> PublishJobProgressAsync(IDomainEventPublisher domainEventPublisher, MediaLibraryScanCompositeId compositeKey, int currentProgress, int totalProgress, CancellationToken cancellationToken)
    {
        Result<MediaLibraryScanJobProgress> scanJobProgressResult = MediaLibraryScanJobProgress.Create(currentProgress, totalProgress, "ExtractingMetadata");
        if (scanJobProgressResult.IsFailure)
            return scanJobProgressResult.Errors;

        await domainEventPublisher.PublishAsync(new LibraryScanJobProgressChangedDomainEvent(
            Guid.NewGuid(), LibraryId, compositeKey, scanJobProgressResult.Value, DateTime.UtcNow), cancellationToken).ConfigureAwait(false);

        return Result.Success;
    }
}
