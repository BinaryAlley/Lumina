#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Admin.Authorization.Roles.Commands.DeleteRole;
using Lumina.Contracts.Requests.Authorization;
#endregion

namespace Lumina.Application.Common.Mapping.Authorization;

/// <summary>
/// Extension methods for converting <see cref="UpdateRoleRequest"/>.
/// </summary>
public static class DeleteRoleRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="DeleteRoleCommand"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted command.</returns>
    public static DeleteRoleCommand ToCommand(this DeleteRoleRequest request)
    {
        return new DeleteRoleCommand(
            request.RoleId!.Value
        );
    }
}
