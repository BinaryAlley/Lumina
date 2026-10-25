#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory;
using Lumina.Application.Common.Utilities;
using Lumina.Contracts.Responses.Common;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.MusicPlayHistoryBoundedContext.MusicPlayRecordAggregate;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.Playback;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory.Queries.GetRecentlyPlayed;

/// <summary>
/// Handler for the query to get the recently played tracks of the current user.
/// </summary>
public class GetRecentlyPlayedQueryHandler : IQueryHandler<GetRecentlyPlayedQuery, Result<PaginatedResponse<PlayedTrackResponse>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthorizationService _authorizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetRecentlyPlayedQueryHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    public GetRecentlyPlayedQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IAuthorizationService authorizationService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _authorizationService = authorizationService;
    }

    /// <summary>
    /// Handles the query to get the recently played tracks of the current user.
    /// </summary>
    /// <param name="query">The query to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the requested page of recently played tracks, or an error message.
    /// </returns>
    public async Task<Result<PaginatedResponse<PlayedTrackResponse>>> HandleAsync(GetRecentlyPlayedQuery query, CancellationToken cancellationToken)
    {
        // An authenticated request must always carry a user identity.
        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return ApplicationErrors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        Result<IReadOnlyList<MusicPlayRecordEntity>> getRecordsResult = await _unitOfWork.MusicPlayRecordRepository.GetForUserAsync(userId, cancellationToken).ConfigureAwait(false);
        if (getRecordsResult.IsFailure)
            return getRecordsResult.Errors;

        // Only the plays that count as plays are aggregated. The last activity of a track is the end of its most recent play, or the last
        // progress update while that play is still in progress.
        List<PlayedTrackAggregate> aggregates = [.. getRecordsResult.Value
            .Where(record => MusicPlayRecord.DoesCountAsPlay(record.Status, record.ListenedDurationMilliseconds.ToClampedTimeSpan()))
            .GroupBy(record => new { record.TrackId, record.LibraryId, record.AlbumId, record.ArtistId })
            .Select(group => new PlayedTrackAggregate(
                group.Key.TrackId,
                group.Key.LibraryId,
                group.Key.AlbumId,
                group.Key.ArtistId,
                group.Max(record => record.EndedOnUtc ?? record.UpdatedOnUtc ?? record.CreatedOnUtc),
                group.Count(),
                group.Count(record => record.Status == PlaybackRecordStatus.Completed),
                group.Sum(record => record.ListenedDurationMilliseconds)))
            .OrderByDescending(aggregate => aggregate.LastActivityOnUtc)
            .ThenBy(aggregate => aggregate.TrackId)];

        return await ToResponsePageAsync(aggregates, query.PaginationData, userId, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Pages the aggregated tracks, resolves their display data, and converts them to the response of the query.
    /// </summary>
    /// <param name="aggregates">The aggregated tracks, already ordered for this query.</param>
    /// <param name="paginationData">The requested pagination data, or <see langword="null"/> for every matching track.</param>
    /// <param name="userId">The Id of the user whose tracks are retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the requested page of played tracks, or an error.</returns>
    private async Task<Result<PaginatedResponse<PlayedTrackResponse>>> ToResponsePageAsync(
        List<PlayedTrackAggregate> aggregates, PaginationDataDto? paginationData, Guid userId, CancellationToken cancellationToken)
    {
        PaginatedResultDto<PlayedTrackAggregate> page = Paginate(aggregates, paginationData);
        Result<List<PlayedTrackDto>> getDtosResult = await ToPlayedTrackDtosAsync(page.Data, userId, cancellationToken).ConfigureAwait(false);
        if (getDtosResult.IsFailure)
            return getDtosResult.Errors;

        return Result.From(new PaginatedResultDto<PlayedTrackDto>
        {
            Data = getDtosResult.Value,
            CurrentPage = page.CurrentPage,
            PerPage = page.PerPage,
            Count = page.Count,
            NumberOfPages = page.NumberOfPages
        }.ToResponses());
    }

    /// <summary>
    /// Applies the requested pagination to the ordered aggregated tracks.
    /// </summary>
    /// <param name="aggregates">The ordered aggregated tracks.</param>
    /// <param name="paginationData">The requested pagination data, or <see langword="null"/> for every matching track.</param>
    /// <returns>The requested page of aggregated tracks.</returns>
    private static PaginatedResultDto<PlayedTrackAggregate> Paginate(List<PlayedTrackAggregate> aggregates, PaginationDataDto? paginationData)
    {
        if (paginationData is null)
            return new PaginatedResultDto<PlayedTrackAggregate>
            {
                Data = aggregates,
                CurrentPage = 1,
                PerPage = aggregates.Count,
                Count = aggregates.Count,
                NumberOfPages = 1
            };

        int count = aggregates.Count;
        int numberOfPages = (int)Math.Ceiling((double)count / paginationData.PerPage);
        int currentPage = Math.Min(paginationData.CurrentPage, Math.Max(1, numberOfPages));
        List<PlayedTrackAggregate> page = [.. aggregates
            .Skip((currentPage - 1) * paginationData.PerPage)
            .Take(paginationData.PerPage)];
        return new PaginatedResultDto<PlayedTrackAggregate>
        {
            Data = page,
            CurrentPage = currentPage,
            PerPage = paginationData.PerPage,
            Count = count,
            NumberOfPages = numberOfPages
        };
    }

    /// <summary>
    /// Resolves the display data of the tracks of <paramref name="aggregates"/> for the libraries the user can access, and builds their DTOs.
    /// </summary>
    /// <param name="aggregates">The aggregated tracks of the requested page.</param>
    /// <param name="userId">The Id of the user whose tracks are retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the played tracks, with blank display data for missing or inaccessible tracks, or an error.</returns>
    private async Task<Result<List<PlayedTrackDto>>> ToPlayedTrackDtosAsync(IReadOnlyList<PlayedTrackAggregate> aggregates, Guid userId, CancellationToken cancellationToken)
    {
        if (aggregates.Count == 0)
            return new List<PlayedTrackDto>();

        // The access decision only needs the library Id that the play record already carries, so it is made once per distinct library before any
        // track or artwork is loaded. Display data from a library the user cannot access is never read.
        Dictionary<Guid, bool> libraryAccess = [];
        foreach (Guid libraryId in aggregates.Select(aggregate => aggregate.LibraryId).Distinct())
            libraryAccess[libraryId] = await _authorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
                userId, new LibraryOwnershipPolicyContext(libraryId), cancellationToken).ConfigureAwait(false);

        List<Guid> accessibleTrackIds = [.. aggregates
            .Where(aggregate => libraryAccess[aggregate.LibraryId])
            .Select(aggregate => aggregate.TrackId)
            .Distinct()];
        Dictionary<Guid, TrackEntity> tracks = [];
        if (accessibleTrackIds.Count > 0)
        {
            Result<IReadOnlyList<TrackEntity>> getTracksResult = await _unitOfWork.TrackRepository.GetByIdsAsync(accessibleTrackIds, shouldIncludeNavigationProperties: true, cancellationToken).ConfigureAwait(false);
            if (getTracksResult.IsFailure)
                return getTracksResult.Errors;
            tracks = getTracksResult.Value.ToDictionary(track => track.Id);
        }

        List<Guid> albumIds = [.. tracks.Values.Select(track => track.AlbumId).Distinct()];
        Dictionary<Guid, string?> albumCovers = [];
        if (albumIds.Count > 0)
        {
            Result<IReadOnlyList<MusicArtworkEntity>> getArtworkResult = await _unitOfWork.MusicArtworkRepository.GetByOwnersAsync(MusicArtworkOwnerType.Album, albumIds, cancellationToken).ConfigureAwait(false);
            if (getArtworkResult.IsFailure)
                return getArtworkResult.Errors;
            albumCovers = getArtworkResult.Value
                .Where(artwork => artwork.ArtworkType == ArtworkType.Cover)
                .GroupBy(artwork => artwork.OwnerId)
                .ToDictionary(
                    group => group.Key,
                    group => group.OrderBy(artwork => artwork.Ordinal).Select(artwork => artwork.FileName).FirstOrDefault());
        }

        List<PlayedTrackDto> playedTracks = [];
        foreach (PlayedTrackAggregate aggregate in aggregates)
        {
            if (libraryAccess[aggregate.LibraryId] && tracks.TryGetValue(aggregate.TrackId, out TrackEntity? foundTrack) && foundTrack.LibraryId == aggregate.LibraryId)
                playedTracks.Add(new PlayedTrackDto(
                    aggregate.TrackId,
                    aggregate.LibraryId,
                    aggregate.AlbumId,
                    aggregate.ArtistId,
                    foundTrack.Title,
                    foundTrack.Album?.Title,
                    foundTrack.Album?.Artist?.Name,
                    albumCovers.TryGetValue(foundTrack.AlbumId, out string? coverFileName) ? coverFileName : null,
                    aggregate.LastActivityOnUtc,
                    aggregate.PlayCount,
                    aggregate.CompletedPlayCount,
                    aggregate.ListenedDurationMilliseconds));
            else // Keep the row so the play counts stay accurate, but blank the display data of a missing or inaccessible track.
                playedTracks.Add(new PlayedTrackDto(
                    aggregate.TrackId,
                    aggregate.LibraryId,
                    aggregate.AlbumId,
                    aggregate.ArtistId,
                    null,
                    null,
                    null,
                    null,
                    aggregate.LastActivityOnUtc,
                    aggregate.PlayCount,
                    aggregate.CompletedPlayCount,
                    aggregate.ListenedDurationMilliseconds));
        }
        return playedTracks;
    }

    /// <summary>
    /// The aggregated counted plays of one track in one library.
    /// </summary>
    /// <param name="TrackId">The Id of the track.</param>
    /// <param name="LibraryId">The Id of the media library that contains the track.</param>
    /// <param name="AlbumId">The Id of the album that contains the track.</param>
    /// <param name="ArtistId">The Id of the artist that produced the track.</param>
    /// <param name="LastActivityOnUtc">The date and time of the last recorded activity of the track's most recent play.</param>
    /// <param name="PlayCount">The number of plays of the track that count as plays.</param>
    /// <param name="CompletedPlayCount">The number of plays of the track that completed.</param>
    /// <param name="ListenedDurationMilliseconds">The duration the user listened to the track, over all of its counted plays, in milliseconds.</param>
    private sealed record PlayedTrackAggregate(
        Guid TrackId,
        Guid LibraryId,
        Guid AlbumId,
        Guid ArtistId,
        DateTime LastActivityOnUtc,
        int PlayCount,
        int CompletedPlayCount,
        long ListenedDurationMilliseconds
    );
}
