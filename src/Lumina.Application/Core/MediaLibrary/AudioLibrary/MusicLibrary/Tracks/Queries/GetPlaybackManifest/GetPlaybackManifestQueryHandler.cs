#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
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

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.GetPlaybackManifest;

/// <summary>
/// Handler for the query to get the playback manifest of a track.
/// </summary>
public class GetPlaybackManifestQueryHandler : IQueryHandler<GetPlaybackManifestQuery, Result<PlaybackManifestResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPlaybackFormatService _playbackFormatService;
    private readonly IPlaybackDeliveryHandleService _playbackDeliveryHandleService;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPathService _pathService;
    private readonly IValidator<GetPlaybackManifestQuery> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPlaybackManifestQueryHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="playbackFormatService">Injected service for deciding how a track's audio is delivered to a player.</param>
    /// <param name="playbackDeliveryHandleService">Injected service that issues the opaque delivery handles of a normalized manifest.</param>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="pathService">Injected service for file system path related functionality.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public GetPlaybackManifestQueryHandler(IUnitOfWork unitOfWork, IPlaybackFormatService playbackFormatService, IPlaybackDeliveryHandleService playbackDeliveryHandleService, IAuthorizationService authorizationService, ICurrentUserService currentUserService, IPathService pathService, IValidator<GetPlaybackManifestQuery> validator)
    {
        _unitOfWork = unitOfWork;
        _playbackFormatService = playbackFormatService;
        _playbackDeliveryHandleService = playbackDeliveryHandleService;
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _pathService = pathService;
        _validator = validator;
    }

    /// <summary>
    /// Handles the query to get the playback manifest of a track.
    /// </summary>
    /// <param name="query">The query to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the playback manifest of the track, or an error message.
    /// </returns>
    public async Task<Result<PlaybackManifestResponse>> HandleAsync(GetPlaybackManifestQuery query, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(query);
        if (validationResult.Count > 0)
            return validationResult;

        // An authenticated request must always carry a user identity.
        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return ApplicationErrors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        // The validator guarantees that the route identifiers are non-empty Guids before this point.
        Guid libraryId = Guid.Parse(query.LibraryId!);
        Guid artistId = Guid.Parse(query.ArtistId!);
        Guid albumId = Guid.Parse(query.AlbumId!);
        Guid trackId = Guid.Parse(query.TrackId!);

        // Admins can build the manifest of the tracks of all libraries; for everyone else, only the tracks of the libraries they own.
        bool canAccessLibrary = await _authorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            userId, new LibraryOwnershipPolicyContext(libraryId), cancellationToken).ConfigureAwait(false);
        if (!canAccessLibrary)
            return ApplicationErrors.Authorization.NotAuthorized;

        // The projection is filtered by the library, the album and the artist named by the route as well as by the track, so a null row
        // means the track either does not exist or is not in scope for the route, which is reported as not found without disclosing anything further.
        Result<TrackPlaybackManifestDto?> getTrackResult = await _unitOfWork.TrackRepository.GetPlaybackManifestInfoAsync(libraryId, albumId, artistId, trackId, cancellationToken).ConfigureAwait(false);
        if (getTrackResult.IsFailure)
            return getTrackResult.Errors;
        if (getTrackResult.Value is null)
            return Errors.Music.TrackNotFound;
        TrackPlaybackManifestDto track = getTrackResult.Value;

        // The stored container and codec of the probe result are the source format the delivery is decided on, and the capabilities
        // the player advertises, or the default capabilities of the bundled player when it advertises none, decide the delivery.
        IReadOnlyList<PlaybackCapabilityDto> capabilities = PlaybackCapabilitiesParser.Parse(query.Capabilities);
        Result<PlaybackDeliveryDecisionDto> decisionResult = _playbackFormatService.DecideDelivery(new PlaybackSourceFormatDto(track.Container, track.AudioCodec), capabilities);
        if (decisionResult.IsFailure)
            return decisionResult.Errors;
        PlaybackDeliveryDecisionDto decision = decisionResult.Value;

        bool isDirectPlay = decision.Mode == PlaybackDeliveryMode.DirectPlay;
        // Whether the delivered audio can be seeked is a property of the shape it is delivered in, so the rule lives on the shape itself.
        bool isSeekable = decision.Shape.IsSeekable();

        string? deliveryId = null;
        if (!isDirectPlay)
        {
            // The quality cap and the source file version are captured now, so the segment route never recomputes the delivery from a
            // setting the user may change, or from a file that may be replaced, while the track is playing.
            PlaybackStreamingQuality quality = PlaybackStreamingQuality.Auto;
            Result<UserSettingsEntity?> getSettingsResult = await _unitOfWork.UserSettingsRepository.GetByUserIdAsync(userId, cancellationToken).ConfigureAwait(false);
            if (getSettingsResult.IsFailure)
                return getSettingsResult.Errors;
            if (getSettingsResult.Value is not null)
                quality = getSettingsResult.Value.StreamingQuality;

            // The content locations are captured with the delivery, so the segment route does not have to load the library and the track again
            // for every six second segment, and it still re-checks path containment on every request.
            Result<LibraryEntity?> getLibraryResult = await _unitOfWork.LibraryRepository.GetByIdAsync(libraryId, shouldTrackEntities: false, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (getLibraryResult.IsFailure)
                return getLibraryResult.Errors;
            if (getLibraryResult.Value is null)
                return Errors.Library.LibraryNotFound;
            IReadOnlyList<string> allowedRoots = [.. getLibraryResult.Value.ContentLocations.Select(contentLocation => contentLocation.Path)];
            // The containment invariant is enforced before the file is even stat'ed, exactly as the stream route does, so a normalized manifest never
            // touches a path outside the content locations of its library. A path that is not inside them is not stat'ed, and the segment route is
            // what rejects it when it tries to resolve the delivery.
            bool isInsideContentLocation = allowedRoots.Count > 0 && allowedRoots.Any(contentLocation => _pathService.IsPathWithin(track.Path, contentLocation));

            // The identity is captured only when the path is inside the library's content locations. A normalized delivery declared for a missing,
            // unreadable or out-of-root file records no identity, and the segment route is the authority that rejects it when the delivery is resolved.
            long sourceLength = 0;
            DateTime sourceLastModifiedUtc = DateTime.MinValue;
            if (isInsideContentLocation)
            {
                try
                {
                    FileInfo sourceFile = new(track.Path);
                    if (sourceFile.Exists)
                    {
                        sourceLength = sourceFile.Length;
                        sourceLastModifiedUtc = sourceFile.LastWriteTimeUtc;
                    }
                }
                catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or NotSupportedException)
                {
                    sourceLength = 0;
                    sourceLastModifiedUtc = DateTime.MinValue;
                }
            }

            NormalizedTrackRequestDto normalizationRequest = new(
                trackId,
                userId,
                track.Path,
                allowedRoots,
                track.Container,
                decision.Mode,
                decision.Container,
                decision.Codec,
                decision.SegmentDurationSeconds,
                track.DurationInSeconds,
                track.Bitrate,
                track.SampleRate,
                track.Channels,
                track.BitDepth,
                quality,
                sourceLength,
                sourceLastModifiedUtc);
            PlaybackDeliveryHandleDto delivery = new(
                normalizationRequest,
                decision.Shape,
                libraryId,
                albumId,
                artistId);
            deliveryId = _playbackDeliveryHandleService.IssueDeliveryId(delivery);
        }

        return new PlaybackManifestResponse(
            trackId,
            track.DurationInSeconds,
            track.Container,
            track.AudioCodec,
            track.SampleRate,
            track.Channels,
            track.BitDepth,
            track.ReplayGainTrackGain,
            track.ReplayGainTrackPeak,
            track.ReplayGainAlbumGain,
            track.ReplayGainAlbumPeak,
            isDirectPlay,
            isDirectPlay ? null : decision.Container,
            isDirectPlay ? null : decision.Codec,
            isDirectPlay ? null : decision.Shape,
            decision.Shape == PlaybackDeliveryShape.TimeAddressedSegments ? decision.SegmentDurationSeconds : null,
            isSeekable,
            query.StreamPath,
            deliveryId,
            deliveryId is null ? null : query.SegmentStreamPathTemplate);
    }
}
