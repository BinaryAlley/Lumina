#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Models.UsersManagement;

/// <summary>
/// Represents the request model for user authentication.
/// </summary>
/// <param name="Username">The username used to authenticate.</param>
/// <param name="Password">The password used to authenticate.</param>
/// <param name="TotpCode">The TOTP (Time-Based One-Time Password) code used for two-factor authentication.</param>
[DebuggerDisplay("Username: {Username}")]
public record LoginRequestModel(
    string? Username = null,
    string? Password = null,
    string? TotpCode = null
);
