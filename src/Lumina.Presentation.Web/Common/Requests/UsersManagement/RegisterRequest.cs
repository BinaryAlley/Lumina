#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.UsersManagement;

/// <summary>
/// Represents the request model for user registration.
/// </summary>
/// <param name="Username">The desired username for the new account. Required.</param>
/// <param name="Password">The password for the new account. Required.</param>
/// <param name="PasswordConfirm">The confirmation of the entered password, used to ensure consistency. Required.</param>
/// <param name="RegistrationType">Specifies the type of registration, e.g., "Admin" or "User". Required.</param>
/// <param name="Use2fa">Indicates whether two-factor authentication (2FA) should be enabled for the new account. Optional.</param>
[DebuggerDisplay("Username: {Username}")]
public record RegisterRequest(
    string? Username,
    string? Password,
    string? PasswordConfirm,
    string? RegistrationType,
    bool Use2fa = true
);
