#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.Scheduling;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Mapping.Scheduling;
using Lumina.Contracts.Responses.Scheduling;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.Scheduling.Queries.GetScheduledJobHistory;

/// <summary>
/// Handler for the query to get the history of the executions of the tasks of scheduled jobs.
/// </summary>
public class GetScheduledJobHistoryQueryHandler : IQueryHandler<GetScheduledJobHistoryQuery, Result<IEnumerable<ScheduledJobExecutionResponse>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthorizationService _authorizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetScheduledJobHistoryQueryHandler"/> class.
    /// </summary>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    public GetScheduledJobHistoryQueryHandler(
        IAuthorizationService authorizationService,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _authorizationService = authorizationService;
    }

    /// <summary>
    /// Handles the query to get the history of the executions of the tasks of scheduled jobs.
    /// </summary>
    /// <param name="query">The query to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the execution history, or an error.
    /// </returns>
    public async Task<Result<IEnumerable<ScheduledJobExecutionResponse>>> HandleAsync(GetScheduledJobHistoryQuery query, CancellationToken cancellationToken)
    {
        // An authenticated request must always carry a user identity.
        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return ApplicationErrors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        // Only administrators can get the history of scheduled jobs.
        if (!await _authorizationService.IsInRoleAsync(userId, "Admin", cancellationToken).ConfigureAwait(false))
            return ApplicationErrors.Authorization.NotAuthorized;

        // The lower bound defaults to one day before the upper bound, which defaults to now.
        DateTime toUtc = query.To ?? DateTime.UtcNow;
        DateTime fromUtc = query.From ?? toUtc.AddDays(-1);

        // Get the executions from the storage medium.
        Result<IEnumerable<ScheduledJobExecutionEntity>> getExecutionsResult = await _unitOfWork.ScheduledJobExecutionRepository.GetByTimeRangeAsync(fromUtc, toUtc, cancellationToken).ConfigureAwait(false);
        if (getExecutionsResult.IsFailure)
            return getExecutionsResult.Errors;

        return Result.From(getExecutionsResult.Value.Select(execution => execution.ToResponse()));
    }
}
