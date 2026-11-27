#region ========================================================================= USING =====================================================================================
using Nemera.Application.Core.Admin.Authorization.Roles.Commands.UpdateRole;
using Nemera.Contracts.Requests.Authorization;
#endregion

namespace Nemera.Application.Common.Mapping.Authorization;

/// <summary>
/// Extension methods for converting <see cref="UpdateRoleRequest"/>.
/// </summary>
public static class UpdateRoleRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="UpdateRoleCommand"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted command.</returns>
    public static UpdateRoleCommand ToCommand(this UpdateRoleRequest request)
    {
        return new UpdateRoleCommand(
            request.RoleId!.Value,
            request.RoleName!,
            request.Permissions!
        );
    }
}
