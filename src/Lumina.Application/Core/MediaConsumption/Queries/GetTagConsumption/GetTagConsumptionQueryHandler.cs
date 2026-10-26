#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DTO.MediaConsumption;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.ConsumptionActivity;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Mapping.MediaConsumption;
using Lumina.Application.Common.Utilities;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.Common.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.MediaConsumption.Queries.GetTagConsumption;

/// <summary>
/// Handler for the query to get the consumption statistics of the tags of a user.
/// </summary>
public class GetTagConsumptionQueryHandler : IQueryHandler<GetTagConsumptionQuery, Result<IReadOnlyList<TagConsumptionResponse>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IValidator<GetTagConsumptionQuery> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTagConsumptionQueryHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public GetTagConsumptionQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IAuthorizationService authorizationService,
        IValidator<GetTagConsumptionQuery> validator)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _authorizationService = authorizationService;
        _validator = validator;
    }

    /// <summary>
    /// Handles the query to get the consumption statistics of the tags of a user.
    /// </summary>
    /// <param name="query">The query to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the requested tag statistics, or an error message.
    /// </returns>
    public async Task<Result<IReadOnlyList<TagConsumptionResponse>>> HandleAsync(GetTagConsumptionQuery query, CancellationToken cancellationToken)
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

        // The access decision only needs the library Id that the records already carry, so it is made once per distinct library before any tag is
        // loaded. Tags of a library the caller cannot access are never read.
        Dictionary<Guid, bool> libraryAccess = [];
        foreach (Guid recordLibraryId in getRecordsResult.Value.Select(record => record.LibraryId).Distinct())
            libraryAccess[recordLibraryId] = await _authorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
                userId, new LibraryOwnershipPolicyContext(recordLibraryId), cancellationToken).ConfigureAwait(false);

        // Only the distinct item Ids of each kind in accessible libraries are read, through the catalogue repositories, in bounded chunks.
        List<Guid> trackIds = [.. getRecordsResult.Value
            .Where(record => record.MediaKind == MediaKind.MusicTrack && libraryAccess[record.LibraryId])
            .Select(record => record.MediaItemId)
            .Distinct()];
        List<TrackEntity> tracks = [];
        if (trackIds.Count > 0)
        {
            Result<IReadOnlyList<TrackEntity>> getTracksResult = await _unitOfWork.TrackRepository
                .GetByIdsAsync(trackIds, shouldIncludeNavigationProperties: true, cancellationToken).ConfigureAwait(false);
            if (getTracksResult.IsFailure)
                return getTracksResult.Errors;
            tracks = [.. getTracksResult.Value];
        }

        List<Guid> bookIds = [.. getRecordsResult.Value
            .Where(record => record.MediaKind == MediaKind.Book && libraryAccess[record.LibraryId])
            .Select(record => record.MediaItemId)
            .Distinct()];
        List<BookEntity> books = [];
        if (bookIds.Count > 0)
        {
            Result<IReadOnlyList<BookEntity>> getBooksResult = await _unitOfWork.BookRepository
                .GetByIdsAsync(bookIds, shouldIncludeNavigationProperties: true, cancellationToken).ConfigureAwait(false);
            if (getBooksResult.IsFailure)
                return getBooksResult.Errors;
            books = [.. getBooksResult.Value];
        }

        Dictionary<(MediaKind MediaKind, Guid MediaItemId, Guid LibraryId), List<string>> tagNamesByItem = [];
        foreach (TrackEntity track in tracks)
        {
            List<string> itemTags = GetTrackTagNames(track);
            if (itemTags.Count > 0)
                tagNamesByItem[(MediaKind.MusicTrack, track.Id, track.LibraryId)] = itemTags;
        }
        foreach (BookEntity book in books)
        {
            List<string> itemTags = [.. book.Tags.Select(tag => tag.Name).OfType<string>()];
            if (itemTags.Count > 0)
                tagNamesByItem[(MediaKind.Book, book.Id, book.LibraryId)] = itemTags;
        }

        // The records are aggregated to one row per item and library first, so an item with several sessions contributes its tags once, not once per
        // session. The library is part of the key because the item is resolved in its current library, and a record from another library must not
        // borrow the tags or the access decision of the current one. An item that moved libraries after consumption is therefore excluded from the
        // breakdown, matching the history lists that blank display data when the record and item libraries disagree.
        Dictionary<string, (int ConsumedItemCount, long ConsumedDurationMilliseconds)> tags = [];
        foreach (IGrouping<(MediaKind MediaKind, Guid MediaItemId, Guid LibraryId), MediaConsumptionRecordEntity> itemGroup in getRecordsResult.Value
            .GroupBy(record => (record.MediaKind, record.MediaItemId, record.LibraryId)))
        {
            if (!libraryAccess[itemGroup.Key.LibraryId])
                continue;
            if (!tagNamesByItem.TryGetValue((itemGroup.Key.MediaKind, itemGroup.Key.MediaItemId, itemGroup.Key.LibraryId), out List<string>? itemTags) ||
                itemTags.Count == 0)
                continue;

            // The breakdown mirrors the statistics, so an item whose sessions all fall below the counting threshold of their media kind does not
            // contribute its tags or its duration to the breakdown.
            Optional<MediaConsumptionRule> ruleResult = MediaConsumptionRule.For(itemGroup.Key.MediaKind);
            if (!ruleResult.HasValue)
                continue;
            bool hasCountedSession = itemGroup.Any(record => ruleResult.Value.Counts(
                record.Status,
                record.ConsumedDurationMilliseconds.ToClampedTimeSpan(),
                record.ConsecutivePagesRead));
            if (!hasCountedSession)
                continue;

            long consumedDurationMilliseconds = itemGroup.Sum(record => record.ConsumedDurationMilliseconds);
            AddTags(tags, itemTags, consumedDurationMilliseconds);
        }

        List<TagConsumptionDto> tagConsumptions = [.. tags
            .Select(tag => new TagConsumptionDto(
                tag.Key,
                tag.Value.ConsumedItemCount,
                tag.Value.ConsumedDurationMilliseconds))
            .OrderByDescending(tagConsumption => tagConsumption.ConsumedItemCount)
            .ThenByDescending(tagConsumption => tagConsumption.ConsumedDurationMilliseconds)
            .ThenBy(tagConsumption => tagConsumption.Name)];

        return Result.From(tagConsumptions.ToResponses());
    }

    /// <summary>
    /// Gets the tag names of <paramref name="track"/>, falling back to the tags of its album and then its artist.
    /// </summary>
    /// <param name="track">The track whose tags are resolved.</param>
    /// <returns>The tag names of the track, its album, or its artist, in the first level that has any.</returns>
    private static List<string> GetTrackTagNames(TrackEntity track)
    {
        List<string> tagNames = [.. track.Tags.Select(tag => tag.Name).OfType<string>()];
        if (tagNames.Count == 0 && track.Album is not null)
            tagNames = [.. track.Album.Tags.Select(tag => tag.Name).OfType<string>()];
        if (tagNames.Count == 0 && track.Album is not null && track.Album.Artist is not null)
            tagNames = [.. track.Album.Artist.Tags.Select(tag => tag.Name).OfType<string>()];
        return tagNames;
    }

    /// <summary>
    /// Adds the lowercase <paramref name="names"/> of one item to the accumulated <paramref name="tags"/>.
    /// </summary>
    /// <param name="tags">The accumulated tags, keyed by lowercase name.</param>
    /// <param name="names">The tag names carried by the item.</param>
    /// <param name="consumedDurationMilliseconds">The duration consumed in the item, over all of its sessions, in milliseconds.</param>
    private static void AddTags(
        Dictionary<string, (int ConsumedItemCount, long ConsumedDurationMilliseconds)> tags,
        List<string> names,
        long consumedDurationMilliseconds)
    {
        // The item is aggregated to one row, so each distinct lowercase name of the item is counted once, regardless of how many sessions it had.
        foreach (string name in names.Select(name => name.Trim().ToLowerInvariant()).Distinct())
        {
            if (name.Length == 0)
                continue;
            if (tags.TryGetValue(name, out (int ConsumedItemCount, long ConsumedDurationMilliseconds) current))
                tags[name] = (current.ConsumedItemCount + 1, current.ConsumedDurationMilliseconds + consumedDurationMilliseconds);
            else
                tags[name] = (1, consumedDurationMilliseconds);
        }
    }
}
