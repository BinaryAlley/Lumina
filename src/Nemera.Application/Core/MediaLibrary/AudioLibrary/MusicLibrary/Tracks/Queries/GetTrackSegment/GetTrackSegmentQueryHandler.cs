#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Lumina.Domain.SharedKernel.Common.Enums.Playback;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.GetTrackSegment;

/// <summary>
/// Handler for the query to get a single segment of the normalized audio of a track, for streaming.
/// </summary>
public class GetTrackSegmentQueryHandler : IQueryHandler<GetTrackSegmentQuery, Result<NormalizedTrackSegmentDataDto>>
{
    private readonly ITrackNormalizationService _trackNormalizationService;
    private readonly IPlaybackDeliveryHandleService _playbackDeliveryHandleService;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPathService _pathService;
    private readonly IValidator<GetTrackSegmentQuery> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTrackSegmentQueryHandler"/> class.
    /// </summary>
    /// <param name="trackNormalizationService">Injected service that produces, caches and serves the normalized audio of a track.</param>
    /// <param name="playbackDeliveryHandleService">Injected service that resolves the opaque delivery handle of the playback manifest.</param>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="pathService">Injected service for file system path related functionality.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public GetTrackSegmentQueryHandler(ITrackNormalizationService trackNormalizationService, IPlaybackDeliveryHandleService playbackDeliveryHandleService, IAuthorizationService authorizationService, ICurrentUserService currentUserService, IPathService pathService, IValidator<GetTrackSegmentQuery> validator)
    {
        _trackNormalizationService = trackNormalizationService;
        _playbackDeliveryHandleService = playbackDeliveryHandleService;
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _pathService = pathService;
        _validator = validator;
    }

    /// <summary>
    /// Handles the query to get a single segment of the normalized audio of a track, for streaming.
    /// </summary>
    /// <param name="query">The query to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the data of the requested segment, or an error message.
    /// </returns>
    public async Task<Result<NormalizedTrackSegmentDataDto>> HandleAsync(GetTrackSegmentQuery query, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(query);
        if (validationResult.Count > 0)
            return validationResult;

        // An authenticated request must always carry a user identity.
        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return ApplicationErrors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        // The validator guarantees that the route identifiers and the segment index are well formed before this point.
        Guid libraryId = Guid.Parse(query.LibraryId!);
        Guid artistId = Guid.Parse(query.ArtistId!);
        Guid albumId = Guid.Parse(query.AlbumId!);
        Guid trackId = Guid.Parse(query.TrackId!);
        int segmentIndex = int.Parse(query.SegmentIndex!);

        // Admins can stream the tracks of all libraries; for everyone else, only the tracks of the libraries they own. This is the only database
        // read of a segment request: the track, library and delivery context were resolved at manifest time and are carried by the delivery handle.
        bool canAccessLibrary = await _authorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            userId, new LibraryOwnershipPolicyContext(libraryId), cancellationToken).ConfigureAwait(false);
        if (!canAccessLibrary)
            return ApplicationErrors.Authorization.NotAuthorized;

        // The delivery handle is the only authority on the delivery of the segments: it was issued to this user for this track, and it holds the
        // decision, the quality cap, the source file version and the library context the manifest declared. It is never recomputed from a capability
        // list, a setting or a file that may have changed since the manifest was read.
        Result<PlaybackDeliveryHandleDto> getDeliveryResult = _playbackDeliveryHandleService.GetDelivery(query.DeliveryId, userId, trackId);
        if (getDeliveryResult.IsFailure)
            return getDeliveryResult.Errors;
        PlaybackDeliveryHandleDto delivery = getDeliveryResult.Value;
        if (delivery.LibraryId != libraryId || delivery.AlbumId != albumId || delivery.ArtistId != artistId)
            return Errors.Playback.PlaybackDeliveryNotFound;
        // A direct play is served by the stream route itself, so a delivery handle for it has no segment. The delivery must also be addressed as
        // time addressed segments with a positive segment duration, or the segment route has nothing it can index.
        if (delivery.Request.Mode == PlaybackDeliveryMode.DirectPlay)
            return Errors.Playback.NormalizationIsNotRequiredForDirectPlay;
        if (delivery.Shape != PlaybackDeliveryShape.TimeAddressedSegments || delivery.Request.SegmentDurationSeconds <= 0)
            return Errors.Playback.PlaybackDeliveryChanged;

        // The audio file is only handed to the normalization pipeline when it is stored inside one of the content locations of its library, so
        // that a stored path can never be used to read an arbitrary file of the host; the failure is reported as not found without the path.
        if (!delivery.Request.AllowedRoots.Any(contentLocation => _pathService.IsPathWithin(delivery.Request.SourcePath, contentLocation)))
            return Errors.Music.TrackFileNotFound;

        NormalizedTrackRequestDto normalizationRequest = delivery.Request;

        // The segment is read from a cached production, or the whole track is produced and cached first. A production is atomic, so the first
        // segment request waits for the complete normalized audio, and every later segment request is served from the cache. A file that changed
        // since the manifest declared the delivery is rejected by the service instead of starting a second, incompatible production.
        Result<NormalizedTrackDto> normalizedTrackResult = await _trackNormalizationService.GetOrCreateNormalizedTrackAsync(normalizationRequest, cancellationToken).ConfigureAwait(false);
        if (normalizedTrackResult.IsFailure)
            return normalizedTrackResult.Errors;
        NormalizedTrackDto normalizedTrack = normalizedTrackResult.Value;

        NormalizedTrackSegmentDto? segment = normalizedTrack.Segments.FirstOrDefault(candidate => candidate.Index == segmentIndex);
        if (segment is null)
            return Errors.Playback.NormalizedTrackSegmentNotFound;

        Result<Stream> openSegmentResult = _trackNormalizationService.OpenNormalizedSegment(normalizedTrack.CacheId, trackId, segmentIndex);
        if (openSegmentResult.IsFailure)
            return openSegmentResult.Errors;

        return new NormalizedTrackSegmentDataDto(openSegmentResult.Value, segment.Length, segment.StartSeconds, normalizedTrack.Container, normalizedTrack.Segments.Count, normalizedTrack.DurationInSeconds);
    }
}
