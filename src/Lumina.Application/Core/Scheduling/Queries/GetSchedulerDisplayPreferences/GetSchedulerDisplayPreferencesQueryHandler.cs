#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.Scheduling;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Errors;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Mapping.Scheduling;
using Lumina.Contracts.Responses.Scheduling;
using Lumina.Domain.Common.Primitives;
using System;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.Scheduling.Queries.GetSchedulerDisplayPreferences;

/// <summary>
/// Handler for the query to get the display preferences of the scheduler page of the current user.
/// </summary>
public class GetSchedulerDisplayPreferencesQueryHandler : IQueryHandler<GetSchedulerDisplayPreferencesQuery, Result<SchedulerDisplayPreferencesResponse>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetSchedulerDisplayPreferencesQueryHandler"/> class.
    /// </summary>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    public GetSchedulerDisplayPreferencesQueryHandler(IAuthorizationService authorizationService, ICurrentUserService currentUserService, IUnitOfWork unitOfWork)
    {
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Handles the query to get the display preferences of the scheduler page of the current user.
    /// </summary>
    /// <param name="query">The query to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the display preferences of the scheduler page of the current user, or an error.
    /// </returns>
    public async Task<Result<SchedulerDisplayPreferencesResponse>> HandleAsync(GetSchedulerDisplayPreferencesQuery query, CancellationToken cancellationToken)
    {
        // An authenticated request must always carry a user identity.
        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return ApplicationErrors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        // Only administrators can view the scheduler page.
        if (!await _authorizationService.IsInRoleAsync(userId, "Admin", cancellationToken).ConfigureAwait(false))
            return ApplicationErrors.Authorization.NotAuthorized;

        // Get the display preferences of the scheduler page of the current user from the repository.
        Result<SchedulerDisplayPreferencesEntity?> getDisplayPreferencesResult = await _unitOfWork.SchedulerDisplayPreferencesRepository.GetByUserIdAsync(userId, cancellationToken).ConfigureAwait(false);
        if (getDisplayPreferencesResult.IsFailure)
            return getDisplayPreferencesResult.Errors;

        // When no display preferences are stored for the current user yet, return the default display preferences.
        if (getDisplayPreferencesResult.Value is null)
            return getDisplayPreferencesResult.Value.ToDefaultResponse(userId);

        return getDisplayPreferencesResult.Value.ToResponse();
    }
}
