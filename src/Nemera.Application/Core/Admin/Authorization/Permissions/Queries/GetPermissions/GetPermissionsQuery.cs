#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
#endregion

namespace Lumina.Application.Core.Admin.Authorization.Permissions.Queries.GetPermissions;

/// <summary>
/// Query for retrieving the list of authorization permissions.
/// </summary>
public record GetPermissionsQuery : IQuery;
