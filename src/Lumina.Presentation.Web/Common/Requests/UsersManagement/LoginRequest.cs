#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.UsersManagement;

/// <summary>
/// Represents the request model for user authentication.
/// </summary>
/// <param name="Username">The username of the account to authenticate. Required.</param>
/// <param name="Password">The password of the account to authenticate. Required.</param>
/// <param name="TotpCode">The TOTP (Time-Based One-Time Password) of the account to authenticate. Optional.</param>
/// <param name="ReturnUrl">The URL to return to, after login.</param>
[DebuggerDisplay("Username: {Username}")]
public record LoginRequest(
    string? Username = null,
    string? Password = null,
    string? TotpCode = null,
    string? ReturnUrl = null
);
