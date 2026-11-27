#region ========================================================================= USING =====================================================================================
using Nemera.Application.Core.Admin.Authorization.Roles.Queries.GetRolePermissions;
using Nemera.Contracts.Requests.Authorization;
#endregion

namespace Nemera.Application.Common.Mapping.Authorization;

/// <summary>
/// Extension methods for converting <see cref="GetRolePermissionsRequest"/>.
/// </summary>
public static class GetRolePermissionsRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="GetRolePermissionsQuery"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted query.</returns>
    public static GetRolePermissionsQuery ToQuery(this GetRolePermissionsRequest request)
    {
        return new GetRolePermissionsQuery(
            request.RoleId
        );
    }
}
