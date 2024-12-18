#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Responses.Authorization;
using Mediator;
using System.Collections.Generic;
#endregion

namespace Lumina.Application.Core.Admin.Authorization.Roles.Queries.GetRoles;

/// <summary>
/// Query for retrieving the list of authorization roles.
/// </summary>
public record GetRolesQuery() : IRequest<ErrorOr<IEnumerable<RoleResponse>>>;
