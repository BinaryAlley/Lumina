#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
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
using Lumina.Domain.SharedKernel.Common.Enums.Playback;
using System;
using System.Collections.Generic;
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
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IValidator<GetPlaybackManifestQuery> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPlaybackManifestQueryHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="playbackFormatService">Injected service for deciding how a track's audio is delivered to a player.</param>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public GetPlaybackManifestQueryHandler(IUnitOfWork unitOfWork, IPlaybackFormatService playbackFormatService, IAuthorizationService authorizationService, ICurrentUserService currentUserService, IValidator<GetPlaybackManifestQuery> validator)
    {
        _unitOfWork = unitOfWork;
        _playbackFormatService = playbackFormatService;
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
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
            query.StreamPath);
    }
}
