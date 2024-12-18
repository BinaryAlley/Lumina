#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Responses.Authorization;
using Mediator;
using System.Collections.Generic;
#endregion

namespace Lumina.Application.Core.Admin.Authorization.Permissions.Queries.GetPermissions;

/// <summary>
/// Query for retrieving the list of authorization permissions.
/// </summary>
public record GetPermissionsQuery : IRequest<ErrorOr<IEnumerable<PermissionResponse>>>;
