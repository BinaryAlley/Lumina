#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.Repositories.Users;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Errors;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Mapping.UsersManagement.Users;
using Lumina.Contracts.Responses.UsersManagement.Settings;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserSettingsAggregate;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.UsersManagement.Settings.Queries.GetUserSettings;

/// <summary>
/// Handler for the query to get the settings of the current user.
/// </summary>
public class GetUserSettingsQueryHandler : IQueryHandler<GetUserSettingsQuery, Result<UserSettingsResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserSettingsQueryHandler"/> class.
    /// </summary>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    public GetUserSettingsQueryHandler(ICurrentUserService currentUserService, IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Handles the query to get the settings of the current user.
    /// </summary>
    /// <param name="query">The query to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully retrieved <see cref="UserSettingsResponse"/>, or an error message.
    /// </returns>
    public async Task<Result<UserSettingsResponse>> HandleAsync(GetUserSettingsQuery query, CancellationToken cancellationToken)
    {
        // an authenticated request must always carry a user identity
        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return Errors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        // get the settings of the current user from the repository
        Result<UserSettingsEntity?> getSettingsResult = await _unitOfWork.UserSettingsRepository.GetByUserIdAsync(userId, cancellationToken).ConfigureAwait(false);
        if (getSettingsResult.IsFailure)
            return getSettingsResult.Errors;

        // when no settings are stored for the current user yet, return the default settings
        if (getSettingsResult.Value is null)
        {
            Result<UserSettings> defaultSettingsResult = UserSettings.Create();
            if (defaultSettingsResult.IsFailure)
                return defaultSettingsResult.Errors;
            return defaultSettingsResult.Value.ToResponse();
        }

        return getSettingsResult.Value.ToResponse();
    }
}
