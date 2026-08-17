#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.UsersManagement;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Authorization;

/// <summary>
/// Data transfer object for a user with its authorization permissions.
/// </summary>
/// <param name="User">The user.</param>
/// <param name="Permissions">The authorization permissions of the user.</param>
[DebuggerDisplay("User: {User}")]
public record UserPermissionsDto(
    UserDto User,
    PermissionDto[] Permissions
);
