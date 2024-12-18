#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.DataAccess.Repositories.Authorization;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Errors;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Mapping.Authorization;
using Lumina.Contracts.Responses.Authorization;
using Mediator;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.Admin.Authorization.Permissions.Queries.GetPermissions;

/// <summary>
/// Handler for the query to retrieve the list of authorization permissions.
/// </summary>
public class GetPermissionsQueryHandler : IRequestHandler<GetPermissionsQuery, ErrorOr<IEnumerable<PermissionResponse>>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IPermissionRepository _permissionRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPermissionsQueryHandler"/> class.
    /// </summary>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    public GetPermissionsQueryHandler(IAuthorizationService authorizationService, ICurrentUserService currentUserService, IUnitOfWork unitOfWork)
    {
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _permissionRepository = unitOfWork.GetRepository<IPermissionRepository>();
    }

    /// <summary>
    /// Handles the query to retrieve the list of authorization permissions.
    /// </summary>
    /// <param name="request">The request to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a collection of <see cref="PermissionResponse"/>, or an error message.
    /// </returns>
    public async ValueTask<ErrorOr<IEnumerable<PermissionResponse>>> Handle(GetPermissionsQuery request, CancellationToken cancellationToken)
    {
        // only admins can see the list of authorization permissions
        bool isAdmin = await _authorizationService.IsInRoleAsync(_currentUserService.UserId!.Value, "Admin", cancellationToken).ConfigureAwait(false);
        if (!isAdmin)
            return Errors.Authorization.NotAuthorized;
        ErrorOr<IEnumerable<PermissionEntity>> getPermissionsResult = await _permissionRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        return getPermissionsResult.Match(value => ErrorOrFactory.From(value.ToResponses()), errors => errors);
    }
}
