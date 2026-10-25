#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Common.DataAccess.Entities.MediaContributors;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DTO.MediaConsumption;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.ConsumptionActivity;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Mapping.MediaConsumption;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Application.Common.Utilities;
using Lumina.Contracts.Responses.Common;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.Common.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.MediaConsumption.Queries.GetMostConsumed;

/// <summary>
/// Handler for the query to get the most consumed media items of a user.
/// </summary>
public class GetMostConsumedQueryHandler : IQueryHandler<GetMostConsumedQuery, Result<PaginatedResponse<ConsumedItemResponse>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IValidator<GetMostConsumedQuery> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetMostConsumedQueryHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public GetMostConsumedQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IAuthorizationService authorizationService,
        IValidator<GetMostConsumedQuery> validator)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _authorizationService = authorizationService;
        _validator = validator;
    }

    /// <summary>
    /// Handles the query to get the most consumed media items of a user.
    /// </summary>
    /// <param name="query">The query to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the requested page of most consumed media items, or an error message.
    /// </returns>
    public async Task<Result<PaginatedResponse<ConsumedItemResponse>>> HandleAsync(GetMostConsumedQuery query, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(query);
        if (validationResult.Count > 0)
            return validationResult;

        // An authenticated request must always carry a user identity.
        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return ApplicationErrors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        // The target defaults to the current user, and the access policy is the only place that decides whether the caller may read it.
        Guid targetUserId = query.TargetUserId is null ? userId : Guid.Parse(query.TargetUserId);
        bool canAccessTarget = await _authorizationService.EvaluatePolicyAsync<IConsumptionActivityAccessPolicy>(
            userId, new ConsumptionActivityAccessPolicyContext(targetUserId), cancellationToken).ConfigureAwait(false);
        if (!canAccessTarget)
            return ApplicationErrors.Authorization.NotAuthorized;

        Guid? libraryId = query.LibraryId is null ? null : Guid.Parse(query.LibraryId);

        Result<IReadOnlyList<MediaConsumptionRecordEntity>> getRecordsResult = await _unitOfWork.ConsumptionStatisticsRepository
            .GetForUserAsync(targetUserId, libraryId, query.MediaKind, query.FromUtc, query.ToUtc, cancellationToken).ConfigureAwait(false);
        if (getRecordsResult.IsFailure)
            return getRecordsResult.Errors;

        // The sessions are reduced to one row per media item before ordering and pagination, so an item consumed many times appears once.
        List<ConsumedItemAggregate> orderedAggregates = [.. BuildAggregates(getRecordsResult.Value)
            .OrderByDescending(aggregate => aggregate.CountedSessionCount)
            .ThenByDescending(aggregate => aggregate.ConsumedDurationMilliseconds)
            .ThenByDescending(aggregate => aggregate.LastConsumedOnUtc)
            .ThenBy(aggregate => aggregate.MediaItemId)
            .ThenBy(aggregate => aggregate.MediaKind)
            .ThenBy(aggregate => aggregate.LibraryId)];

        PaginatedResultDto<ConsumedItemDto> page;
        if (query.PaginationData is null)
        {
            // Without pagination the caller asked for every matching item, which is bounded by the number of distinct items in the history.
            Result<List<ConsumedItemDto>> getItemsResult = await ToConsumedItemDtosAsync(orderedAggregates, userId, cancellationToken).ConfigureAwait(false);
            if (getItemsResult.IsFailure)
                return getItemsResult.Errors;
            page = new PaginatedResultDto<ConsumedItemDto>
            {
                Data = getItemsResult.Value,
                CurrentPage = 1,
                PerPage = orderedAggregates.Count,
                Count = orderedAggregates.Count,
                NumberOfPages = 1
            };
        }
        else
        {
            int currentPage = query.PaginationData.CurrentPage;
            int perPage = query.PaginationData.PerPage;
            int count = orderedAggregates.Count;
            int numberOfPages = (int)Math.Ceiling((double)count / perPage);
            int pageNumber = Math.Min(currentPage, Math.Max(1, numberOfPages));
            List<ConsumedItemAggregate> pageAggregates = [.. orderedAggregates
                .Skip((pageNumber - 1) * perPage)
                .Take(perPage)];
            Result<List<ConsumedItemDto>> getItemsResult = await ToConsumedItemDtosAsync(pageAggregates, userId, cancellationToken).ConfigureAwait(false);
            if (getItemsResult.IsFailure)
                return getItemsResult.Errors;
            page = new PaginatedResultDto<ConsumedItemDto>
            {
                Data = getItemsResult.Value,
                CurrentPage = pageNumber,
                PerPage = perPage,
                Count = count,
                NumberOfPages = numberOfPages
            };
        }

        return Result.From(page.ToResponses());
    }

    /// <summary>
    /// Aggregates the consumption records of each media item into the counts and duration that the history lists show.
    /// </summary>
    /// <param name="records">The consumption records of the user, already filtered by the query.</param>
    /// <returns>One aggregate per media item, library, and media kind.</returns>
    private static List<ConsumedItemAggregate> BuildAggregates(IReadOnlyList<MediaConsumptionRecordEntity> records)
    {
        List<ConsumedItemAggregate> aggregates = [];
        foreach (IGrouping<(MediaKind MediaKind, Guid MediaItemId, Guid LibraryId), MediaConsumptionRecordEntity> itemGroup in records
            .GroupBy(record => (record.MediaKind, record.MediaItemId, record.LibraryId)))
        {
            // Counted sessions follow the rule of the media kind, while the completed count is the subset that completed. Duration totals every
            // session of the item, because it measures consumed time, not only the sessions that count.
            Optional<MediaConsumptionRule> ruleResult = MediaConsumptionRule.For(itemGroup.Key.MediaKind);
            int countedSessionCount = 0;
            int completedSessionCount = 0;
            long consumedDurationMilliseconds = 0;
            DateTime lastConsumedOnUtc = DateTime.MinValue;
            foreach (MediaConsumptionRecordEntity record in itemGroup)
            {
                consumedDurationMilliseconds += record.ConsumedDurationMilliseconds;
                if (record.LastActivityOnUtc > lastConsumedOnUtc)
                    lastConsumedOnUtc = record.LastActivityOnUtc;
                if (record.Status == ConsumptionStatus.Completed)
                    completedSessionCount++;
                if (ruleResult.HasValue &&
                    ruleResult.Value.Counts(record.Status, record.ConsumedDurationMilliseconds.ToClampedTimeSpan(), record.ConsecutivePagesRead))
                    countedSessionCount++;
            }
            aggregates.Add(new ConsumedItemAggregate(
                itemGroup.Key.MediaKind,
                itemGroup.Key.MediaItemId,
                itemGroup.Key.LibraryId,
                lastConsumedOnUtc,
                countedSessionCount,
                completedSessionCount,
                consumedDurationMilliseconds));
        }
        return aggregates;
    }

    /// <summary>
    /// Resolves the display data of the media items of <paramref name="aggregates"/> and builds their DTOs.
    /// </summary>
    /// <param name="aggregates">The aggregated history rows of the requested page.</param>
    /// <param name="currentUserId">The Id of the current user, used to decide which library display data may be shown.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The history rows, with blank display data for missing or inaccessible items.</returns>
    private async Task<Result<List<ConsumedItemDto>>> ToConsumedItemDtosAsync(
        List<ConsumedItemAggregate> aggregates, Guid currentUserId, CancellationToken cancellationToken)
    {
        if (aggregates.Count == 0)
            return new List<ConsumedItemDto>();

        // The access decision only needs the library Id that the record already carries, so it is made once per distinct library before any item
        // or artwork is loaded. Inaccessible items keep their counts and duration, but their display data is never read.
        Dictionary<Guid, bool> libraryAccess = [];
        foreach (Guid libraryId in aggregates.Select(aggregate => aggregate.LibraryId).Distinct())
            libraryAccess[libraryId] = await _authorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
                currentUserId, new LibraryOwnershipPolicyContext(libraryId), cancellationToken).ConfigureAwait(false);

        List<Guid> trackIds = [.. aggregates
            .Where(aggregate => aggregate.MediaKind == MediaKind.MusicTrack && libraryAccess[aggregate.LibraryId])
            .Select(aggregate => aggregate.MediaItemId)
            .Distinct()];
        Dictionary<Guid, TrackEntity> tracks = [];
        if (trackIds.Count > 0)
        {
            Result<IReadOnlyList<TrackEntity>> getTracksResult = await _unitOfWork.TrackRepository
                .GetByIdsAsync(trackIds, shouldIncludeNavigationProperties: true, cancellationToken).ConfigureAwait(false);
            if (getTracksResult.IsFailure)
                return getTracksResult.Errors;
            tracks = getTracksResult.Value.ToDictionary(track => track.Id);
        }

        List<Guid> albumIds = [.. tracks.Values.Select(track => track.AlbumId).Distinct()];
        Dictionary<Guid, string?> albumCovers = [];
        if (albumIds.Count > 0)
        {
            Result<IReadOnlyList<MusicArtworkEntity>> getArtworkResult = await _unitOfWork.MusicArtworkRepository
                .GetByOwnersAsync(MusicArtworkOwnerType.Album, albumIds, cancellationToken).ConfigureAwait(false);
            if (getArtworkResult.IsFailure)
                return getArtworkResult.Errors;
            albumCovers = getArtworkResult.Value
                .Where(artwork => artwork.ArtworkType == ArtworkType.Cover)
                .GroupBy(artwork => artwork.OwnerId)
                .ToDictionary(
                    group => group.Key,
                    group => group.OrderBy(artwork => artwork.Ordinal).Select(artwork => artwork.FileName).FirstOrDefault());
        }

        List<Guid> bookIds = [.. aggregates
            .Where(aggregate => aggregate.MediaKind == MediaKind.Book && libraryAccess[aggregate.LibraryId])
            .Select(aggregate => aggregate.MediaItemId)
            .Distinct()];
        Dictionary<Guid, BookEntity> books = [];
        if (bookIds.Count > 0)
        {
            Result<IReadOnlyList<BookEntity>> getBooksResult = await _unitOfWork.BookRepository
                .GetByIdsAsync(bookIds, shouldIncludeNavigationProperties: true, cancellationToken).ConfigureAwait(false);
            if (getBooksResult.IsFailure)
                return getBooksResult.Errors;
            books = getBooksResult.Value.ToDictionary(book => book.Id);
        }

        List<Guid> authorIds = [.. books.Values
            .SelectMany(book => book.Contributors
                .Where(contributor => contributor.Role == MediaContributorRole.Author)
                .Select(contributor => contributor.MediaContributorId))
            .Distinct()];
        Dictionary<Guid, string> authorNames = [];
        if (authorIds.Count > 0)
        {
            Result<IReadOnlyList<MediaContributorEntity>> getAuthorsResult = await _unitOfWork.MediaContributorRepository
                .GetByIdsAsync(authorIds, cancellationToken).ConfigureAwait(false);
            if (getAuthorsResult.IsFailure)
                return getAuthorsResult.Errors;
            authorNames = getAuthorsResult.Value.ToDictionary(contributor => contributor.Id, contributor => contributor.DisplayName);
        }

        List<ConsumedItemDto> items = [];
        foreach (ConsumedItemAggregate aggregate in aggregates)
        {
            if (aggregate.MediaKind == MediaKind.MusicTrack &&
                libraryAccess[aggregate.LibraryId] &&
                tracks.TryGetValue(aggregate.MediaItemId, out TrackEntity? track) &&
                track.LibraryId == aggregate.LibraryId)
            {
                string? coverPath = albumCovers.TryGetValue(track.AlbumId, out string? coverFileName) ? coverFileName : null;
                items.Add(track.ToDto(
                    aggregate.LastConsumedOnUtc,
                    aggregate.CountedSessionCount,
                    aggregate.CompletedSessionCount,
                    aggregate.ConsumedDurationMilliseconds,
                    coverPath));
                continue;
            }

            if (aggregate.MediaKind == MediaKind.Book &&
                libraryAccess[aggregate.LibraryId] &&
                books.TryGetValue(aggregate.MediaItemId, out BookEntity? book) &&
                book.LibraryId == aggregate.LibraryId)
            {
                string creatorName = book.Contributors
                    .Where(contributor => contributor.Role == MediaContributorRole.Author && authorNames.ContainsKey(contributor.MediaContributorId))
                    .Select(contributor => authorNames[contributor.MediaContributorId])
                    .OrderBy(name => name)
                    .FirstOrDefault() ?? string.Empty;
                items.Add(book.ToDto(
                    aggregate.LastConsumedOnUtc,
                    aggregate.CountedSessionCount,
                    aggregate.CompletedSessionCount,
                    aggregate.ConsumedDurationMilliseconds,
                    creatorName));
                continue;
            }

            // A deleted or inaccessible item keeps its counts and duration, because the sessions were really consumed, but its display data is blank.
            items.Add(new ConsumedItemDto(
                aggregate.MediaKind,
                aggregate.MediaItemId,
                aggregate.LibraryId,
                false,
                string.Empty,
                string.Empty,
                string.Empty,
                null,
                aggregate.LastConsumedOnUtc,
                aggregate.CountedSessionCount,
                aggregate.CompletedSessionCount,
                aggregate.ConsumedDurationMilliseconds));
        }
        return items;
    }

    /// <summary>
    /// The aggregated consumption of one media item in one library.
    /// </summary>
    /// <param name="MediaKind">The kind of the consumed media item.</param>
    /// <param name="MediaItemId">The Id of the consumed media item.</param>
    /// <param name="LibraryId">The Id of the media library that contains the media item.</param>
    /// <param name="LastConsumedOnUtc">The date and time when the media item was last consumed.</param>
    /// <param name="CountedSessionCount">The number of sessions that count towards statistics.</param>
    /// <param name="CompletedSessionCount">The number of sessions that completed.</param>
    /// <param name="ConsumedDurationMilliseconds">The active duration consumed in the media item, over all of its sessions, in milliseconds.</param>
    private sealed record ConsumedItemAggregate(
        MediaKind MediaKind,
        Guid MediaItemId,
        Guid LibraryId,
        DateTime LastConsumedOnUtc,
        int CountedSessionCount,
        int CompletedSessionCount,
        long ConsumedDurationMilliseconds
    );
}
