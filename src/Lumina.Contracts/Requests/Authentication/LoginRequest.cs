#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.Authentication;

/// <summary>
/// Represents the request model for user authentication.
/// </summary>
/// <param name="Username">The username of the account to authenticate.</param>
/// <param name="Password">The password of the account to authenticate.</param>
/// <param name="TotpCode">The TOTP (Time-Based One-Time Password) of the account to authenticate.</param>
[DebuggerDisplay("Username: {Username}")]
public record LoginRequest(
    string? Username,
    string? Password,
    string? TotpCode = null
);
