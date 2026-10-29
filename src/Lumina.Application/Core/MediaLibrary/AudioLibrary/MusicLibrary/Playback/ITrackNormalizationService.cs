#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.Errors;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Playback;

/// <summary>
/// Interface for the service that produces, caches and serves the normalized audio of a track, when the player cannot play the stored audio directly.
/// </summary>
/// <remarks>
/// The caller is responsible for authorizing the request. The service re-validates the source path against the allowed roots carried by the request and
/// re-checks it after opening, as defense in depth, so a caller that forgets containment cannot hand an arbitrary path to the transcoder. The service
/// itself only handles the media pipeline: it never reads or writes a database row, and it never emits a domain event. Every entry is bound to the track
/// it was produced for, so a cache handle can never be used to read the audio of another track.
/// A production is atomic: the cache entry is published only after ffmpeg has finished the whole track, so the first request waits for a complete
/// production. Streaming in this contract means serving the produced segments from the cache without buffering them into memory, not publishing a partial
/// track. Incremental publication of segment 0 while the rest is produced would be a different contract. Admission is bounded: when every normalization job
/// slot is in use, a request for an entry that is not cached yet is reported as unavailable instead of being queued behind the running jobs, and a single
/// user can only occupy a configured number of those slots, so a prefetch or a burst of requests from one user cannot make another user's playback fail.
/// A production that was admitted outlives the request that started it, so repeated aborts converge to a cached entry. A production that needs a toolchain the startup
/// probe found unusable reports <see cref="Errors.Playback.NormalizationUnavailable"/> as well, while an entry that is already
/// cached is still served. The playback stream surface exposes the produced segments to the player through the segment route of the track stream contract.
/// </remarks>
public interface ITrackNormalizationService
{
    /// <summary>
    /// Gets the normalized audio of a track from the cache, or produces it and caches it when it is not cached yet.
    /// </summary>
    /// <param name="request">The track, the requesting user, the delivery decision, the source file and the quality cap the normalized audio is produced with.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the cached normalized audio of the track, or an error message.
    /// </returns>
    /// <exception cref="OperationCanceledException">
    /// Thrown when <paramref name="cancellationToken"/> is canceled while the caller is waiting. The production continues in the background, so this is a
    /// disconnect signal and must not be treated as a production failure.
    /// </exception>
    Task<Result<NormalizedTrackDto>> GetOrCreateNormalizedTrackAsync(NormalizedTrackRequestDto request, CancellationToken cancellationToken);

    /// <summary>
    /// Opens a read stream over a single segment of a cached normalized track, after verifying that the entry belongs to the provided track.
    /// </summary>
    /// <remarks>
    /// A missing or evicted cache entry, a segment that is no longer present, an entry that cannot be read, or a value that is not a well formed handle is
    /// reported with <see cref="Errors.Playback.NormalizedTrackCacheEntryNotFound"/>. That is the caller's signal to request the
    /// playback manifest again, or to trigger a new production, instead of treating the result as a broken transcode. A genuine production failure keeps
    /// its own error. A service that has already been disposed reports <see cref="Errors.Playback.NormalizationUnavailable"/>.
    /// </remarks>
    /// <param name="cacheId">The opaque identifier of the cached entry that holds the normalized audio.</param>
    /// <param name="trackId">The Id of the track the caller authorized and whose segment is opened.</param>
    /// <param name="segmentIndex">The zero based position of the segment in the delivered audio.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a read stream over the segment, or an error message. The caller owns the returned stream.
    /// </returns>
    Result<Stream> OpenNormalizedSegment(string cacheId, Guid trackId, int segmentIndex);
}
