#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.Repositories.Users;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Errors;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Mapping.Authorization;
using Lumina.Contracts.Responses.Authorization;
using Mediator;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.UsersManagement.Authorization.Queries.GetUserRole;

/// <summary>
/// Handler for the query to retrieve the authorization role of a user.
/// </summary>
public class GetUserRoleQueryHandler : IRequestHandler<GetUserRoleQuery, ErrorOr<RoleResponse?>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IUserRepository _userRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserRoleQueryHandler"/> class.
    /// </summary>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    public GetUserRoleQueryHandler(IAuthorizationService authorizationService, ICurrentUserService currentUserService, IUnitOfWork unitOfWork)
    {
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _userRepository = unitOfWork.GetRepository<IUserRepository>();
    }

    /// <summary>
    /// Handles the query to retrieve the authorization role of a user.
    /// </summary>
    /// <param name="request">The request to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a <see cref="RoleResponse"/>, or an error message.
    /// </returns>
    public async ValueTask<ErrorOr<RoleResponse?>> Handle(GetUserRoleQuery request, CancellationToken cancellationToken)
    {
        // only admins can see the authorization role
        bool isAdmin = await _authorizationService.IsInRoleAsync(_currentUserService.UserId!.Value, "Admin", cancellationToken).ConfigureAwait(false);
        if (!isAdmin)
            return Errors.Authorization.NotAuthorized;
        // get the user from the repository and return its roles
        ErrorOr<UserEntity?> getUserResult = await _userRepository.GetByIdAsync(request.UserId!.Value, cancellationToken).ConfigureAwait(false);
        if (getUserResult.IsError)
            return getUserResult.Errors;
        else if (getUserResult.Value is null)
            return Errors.Authentication.UsernameDoesNotExist;
        return ErrorOrFactory.From(getUserResult.Value.UserRole?.Role.ToResponse());
    }
}
