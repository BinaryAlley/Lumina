#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.Admin.Authorization.Roles.Queries.GetRolePermissions;

/// <summary>
/// Query for getting the authorization permissions of a role identified by <paramref name="RoleId"/>.
/// </summary>
/// <param name="RoleId">The unique identifier of the authorization role for which to get the list of permissions.</param>
[DebuggerDisplay("RoleId: {RoleId}")]
public record GetRolePermissionsQuery(
    Guid RoleId
) : IQuery;
