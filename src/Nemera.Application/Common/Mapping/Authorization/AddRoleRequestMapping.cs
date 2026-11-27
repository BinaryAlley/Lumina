#region ========================================================================= USING =====================================================================================
using Nemera.Application.Core.Admin.Authorization.Roles.Commands.AddRole;
using Nemera.Contracts.Requests.Authorization;
#endregion

namespace Nemera.Application.Common.Mapping.Authorization;

/// <summary>
/// Extension methods for converting <see cref="AddRoleRequest"/>.
/// </summary>
public static class AddRoleRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="AddRoleCommand"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted command.</returns>
    public static AddRoleCommand ToCommand(this AddRoleRequest request)
    {
        return new AddRoleCommand(
            request.RoleName!,
            request.Permissions!
        );
    }
}
