#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
using Nemera.Application.Common.DataAccess.Entities.Authorization;
using Nemera.Application.Common.DataAccess.Repositories.Authorization;
using Nemera.Application.Common.DataAccess.UoW;
using Nemera.Application.Common.DTO.Filtering;
using Nemera.Application.Common.DTO.Pagination;
using Nemera.Application.Common.Errors;
using Nemera.Application.Common.Infrastructure.Authentication;
using Nemera.Application.Common.Infrastructure.Authorization;
using Nemera.Application.Common.Mapping.Authorization;
using Nemera.Contracts.Responses.Authorization;
using Nemera.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Application.Core.Admin.Authorization.Roles.Queries.GetRoles;

/// <summary>
/// Handler for the query to retrieve the list of authorization roles.
/// </summary>
public class GetRolesQueryHandler : IQueryHandler<GetRolesQuery, Result<IEnumerable<RoleResponse>>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IRoleRepository _roleRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetRolesQueryHandler"/> class.
    /// </summary>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    public GetRolesQueryHandler(IAuthorizationService authorizationService, ICurrentUserService currentUserService, IUnitOfWork unitOfWork)
    {
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _roleRepository = unitOfWork.RoleRepository;
    }

    /// <summary>
    /// Handles the query to retrieve the list of authorization roles.
    /// </summary>
    /// <param name="query">The request to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a collection of <see cref="RoleResponse"/>, or an error message.
    /// </returns>
    public async Task<Result<IEnumerable<RoleResponse>>> HandleAsync(GetRolesQuery query, CancellationToken cancellationToken)
    {
        // An authenticated request must always carry a user identity.
        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return Errors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        // Only admins can see the list of authorization roles.
        bool isAdmin = await _authorizationService.IsInRoleAsync(userId, AuthorizationRoles.ADMIN, cancellationToken).ConfigureAwait(false);
        if (!isAdmin)
            return Errors.Authorization.NotAuthorized;

        Result<PaginatedResultDto<RoleEntity>> getRolesResult = await _roleRepository.GetAllAsync<BaseFilterDto>(cancellationToken: cancellationToken).ConfigureAwait(false);
        return getRolesResult.Match(value => Result.From(value.Data.ToResponses()), errors => errors);
    }
}
