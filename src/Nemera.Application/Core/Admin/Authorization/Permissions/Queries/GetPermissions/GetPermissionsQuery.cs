#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
#endregion

namespace Nemera.Application.Core.Admin.Authorization.Permissions.Queries.GetPermissions;

/// <summary>
/// Query for retrieving the list of authorization permissions.
/// </summary>
public record GetPermissionsQuery : IQuery;
