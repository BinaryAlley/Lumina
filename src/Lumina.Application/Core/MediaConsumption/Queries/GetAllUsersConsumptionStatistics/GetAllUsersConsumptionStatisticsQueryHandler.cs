#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DTO.MediaConsumption;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Mapping.MediaConsumption;
using Lumina.Application.Common.Utilities;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.Common.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.MediaConsumption.Queries.GetAllUsersConsumptionStatistics;

/// <summary>
/// Handler for the query to get the consumption statistics of every user.
/// </summary>
public class GetAllUsersConsumptionStatisticsQueryHandler : IQueryHandler<GetAllUsersConsumptionStatisticsQuery, Result<AllUsersConsumptionStatisticsResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IValidator<GetAllUsersConsumptionStatisticsQuery> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAllUsersConsumptionStatisticsQueryHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public GetAllUsersConsumptionStatisticsQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IAuthorizationService authorizationService,
        IValidator<GetAllUsersConsumptionStatisticsQuery> validator)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _authorizationService = authorizationService;
        _validator = validator;
    }

    /// <summary>
    /// Handles the query to get the consumption statistics of every user.
    /// </summary>
    /// <param name="query">The query to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the requested <see cref="AllUsersConsumptionStatisticsResponse"/>, or an error message.
    /// </returns>
    public async Task<Result<AllUsersConsumptionStatisticsResponse>> HandleAsync(GetAllUsersConsumptionStatisticsQuery query, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(query);
        if (validationResult.Count > 0)
            return validationResult;

        // An authenticated request must always carry a user identity.
        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return ApplicationErrors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        // Only admins can read the statistics of every user; the all-users summary is not derived from the per-user routes.
        bool isAdmin = await _authorizationService.IsInRoleAsync(userId, AuthorizationRoles.ADMIN, cancellationToken).ConfigureAwait(false);
        if (!isAdmin)
            return ApplicationErrors.Authorization.NotAuthorized;

        Guid? libraryId = query.LibraryId is null ? null : Guid.Parse(query.LibraryId);

        // This is the only statistics query that loads the records of every user, and it is reachable only through the admin check above.
        Result<IReadOnlyList<MediaConsumptionRecordEntity>> getRecordsResult = await _unitOfWork.ConsumptionStatisticsRepository
            .GetForAllUsersAsync(libraryId, query.MediaKind, query.FromUtc, query.ToUtc, cancellationToken).ConfigureAwait(false);
        if (getRecordsResult.IsFailure)
            return getRecordsResult.Errors;

        // The user summaries are built in one pass over the grouped records. The server total is then derived from those summaries, so it can
        // never disagree with the sum of the users and the records are not walked again for it.
        List<ConsumptionStatisticsDto> userSummaries = [.. getRecordsResult.Value
            .GroupBy(record => record.UserId)
            .OrderBy(group => group.Key)
            .Select(group => BuildStatistics(group.Key, group))];

        List<MediaKindConsumptionStatisticsDto> totalMediaKindSummaries = [.. userSummaries
            .SelectMany(summary => summary.MediaKindSummaries)
            .GroupBy(summary => summary.MediaKind)
            .OrderBy(group => group.Key)
            .Select(group =>
            {
                int countedSessionCount = group.Sum(summary => summary.CountedSessionCount);
                int completedSessionCount = group.Sum(summary => summary.CompletedSessionCount);
                return new MediaKindConsumptionStatisticsDto(
                    group.Key,
                    group.Sum(summary => summary.TotalConsumedDurationMilliseconds),
                    countedSessionCount,
                    completedSessionCount,
                    group.Sum(summary => summary.StoppedSessionCount),
                    countedSessionCount == 0 ? 0 : (double)completedSessionCount / countedSessionCount);
            })];

        int totalCountedSessionCount = userSummaries.Sum(summary => summary.CountedSessionCount);
        int totalCompletedSessionCount = userSummaries.Sum(summary => summary.CompletedSessionCount);
        ConsumptionTotalsDto totals = new(
            userSummaries.Sum(summary => summary.TotalConsumedDurationMilliseconds),
            totalCountedSessionCount,
            totalCompletedSessionCount,
            userSummaries.Sum(summary => summary.StoppedSessionCount),
            totalCountedSessionCount == 0 ? 0 : (double)totalCompletedSessionCount / totalCountedSessionCount,
            totalMediaKindSummaries);

        return new AllUsersConsumptionStatisticsDto(totals, userSummaries).ToResponse();
    }

    /// <summary>
    /// Builds the consumption statistics of a user from the records of that user.
    /// </summary>
    /// <param name="userId">The Id of the user the records belong to.</param>
    /// <param name="records">The records of the user, already filtered by the query.</param>
    /// <returns>The consumption statistics of the user.</returns>
    private static ConsumptionStatisticsDto BuildStatistics(Guid userId, IEnumerable<MediaConsumptionRecordEntity> records)
    {
        // The rule is created once per media kind instead of once per record. A completed session always counts, while a stopped session counts
        // only when it reaches the counting threshold of its media kind. Duration totals every session because it measures consumed time, not the
        // subset of sessions that count towards the statistics.
        List<MediaKindConsumptionStatisticsDto> mediaKindSummaries = [];
        foreach (IGrouping<MediaKind, MediaConsumptionRecordEntity> mediaKindGroup in records
            .GroupBy(record => record.MediaKind)
            .OrderBy(group => group.Key))
        {
            Optional<MediaConsumptionRule> ruleResult = MediaConsumptionRule.For(mediaKindGroup.Key);
            int countedSessionCount = 0;
            int completedSessionCount = 0;
            int stoppedSessionCount = 0;
            long consumedDurationMilliseconds = 0;
            foreach (MediaConsumptionRecordEntity record in mediaKindGroup)
            {
                consumedDurationMilliseconds += record.ConsumedDurationMilliseconds;
                if (!ruleResult.HasValue)
                    continue;
                if (record.Status == ConsumptionStatus.Completed)
                    completedSessionCount++;
                if (!ruleResult.Value.Counts(record.Status, record.ConsumedDurationMilliseconds.ToClampedTimeSpan(), record.ConsecutivePagesRead))
                    continue;
                countedSessionCount++;
                if (record.Status == ConsumptionStatus.Stopped)
                    stoppedSessionCount++;
            }
            mediaKindSummaries.Add(new MediaKindConsumptionStatisticsDto(
                mediaKindGroup.Key,
                consumedDurationMilliseconds,
                countedSessionCount,
                completedSessionCount,
                stoppedSessionCount,
                countedSessionCount == 0 ? 0 : (double)completedSessionCount / countedSessionCount));
        }

        // The overall totals are the sums of the per-kind summaries, so the completion rate can never disagree with the breakdown.
        long totalConsumedDurationMilliseconds = mediaKindSummaries.Sum(summary => summary.TotalConsumedDurationMilliseconds);
        int totalCountedSessionCount = mediaKindSummaries.Sum(summary => summary.CountedSessionCount);
        int totalCompletedSessionCount = mediaKindSummaries.Sum(summary => summary.CompletedSessionCount);
        int totalStoppedSessionCount = mediaKindSummaries.Sum(summary => summary.StoppedSessionCount);
        return new ConsumptionStatisticsDto(
            userId,
            totalConsumedDurationMilliseconds,
            totalCountedSessionCount,
            totalCompletedSessionCount,
            totalStoppedSessionCount,
            totalCountedSessionCount == 0 ? 0 : (double)totalCompletedSessionCount / totalCountedSessionCount,
            mediaKindSummaries);
    }
}
