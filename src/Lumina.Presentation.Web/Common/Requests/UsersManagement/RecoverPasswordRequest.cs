#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.UsersManagement;

/// <summary>
/// Represents the request model for account password recovery.
/// </summary>
/// <param name="Username">The username of the account for which to recover the password. Required.</param>
/// <param name="TotpCode">The TOTP (Time-Based One-Time Password) of the account for which to recover the password. Required.</param>
[DebuggerDisplay("Username: {Username}")]
public record RecoverPasswordRequest(
    string? Username,
    string? TotpCode
);
