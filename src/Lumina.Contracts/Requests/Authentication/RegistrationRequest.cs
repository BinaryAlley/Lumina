#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.Authentication;

/// <summary>
/// Represents the request model for user registration.
/// </summary>
/// <param name="Username">The desired username for the new account.</param>
/// <param name="Password">The password for the new account.</param>
/// <param name="PasswordConfirm">The confirmation of the entered password, used to ensure consistency.</param>
/// <param name="Use2fa">Indicates whether two-factor authentication (2FA) should be enabled for the new account.</param>
[DebuggerDisplay("Username: {Username}")]
public record RegistrationRequest(
    string? Username = null,
    string? Password = null,
    string? PasswordConfirm = null,
    bool Use2fa = true
);
