#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.UsersManagement.Authorization.Commands.UpdateUserRoleAndPermissions;
using Lumina.Contracts.Requests.Authorization;
#endregion

namespace Lumina.Application.Common.Mapping.Authorization;

/// <summary>
/// Extension methods for converting <see cref="UpdateUserRoleAndPermissionsRequest"/>.
/// </summary>
public static class UpdateUserRoleAndPermissionsRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="UpdateUserRoleAndPermissionsCommand"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted command.</returns>
    public static UpdateUserRoleAndPermissionsCommand ToCommand(this UpdateUserRoleAndPermissionsRequest request)
    {
        return new UpdateUserRoleAndPermissionsCommand(
            request.UserId!.Value,
            request.RoleId,
            request.Permissions!
        );
    }
}
