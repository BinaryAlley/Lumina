#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.UsersManagement;

/// <summary>
/// Represents the request model for account password change.
/// </summary>
/// <param name="Username">The username of the account. Required.</param>
/// <param name="CurrentPassword">The current password of the account. Required.</param>
/// <param name="NewPassword">The new password of the account. Required.</param>
/// <param name="NewPasswordConfirm">The confirmation of the new password of the account. Required.</param>
[DebuggerDisplay("Username: {Username}")]
public record ChangePasswordRequest(
    string? Username,
    string? CurrentPassword,    
    string? NewPassword,
    string? NewPasswordConfirm
);
