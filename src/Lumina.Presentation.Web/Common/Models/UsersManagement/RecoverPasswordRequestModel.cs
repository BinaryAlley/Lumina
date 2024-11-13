#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Models.UsersManagement;

/// <summary>
/// Represents the request model for account password recovery.
/// </summary>
/// <param name="Username">The username of the account for which to recover the password.</param>
/// <param name="TotpCode">The TOTP (Time-Based One-Time Password) code used to recover the password.</param>
[DebuggerDisplay("Username: {Username}")]
public record RecoverPasswordRequestModel(
    string? Username,
    string? TotpCode
);
