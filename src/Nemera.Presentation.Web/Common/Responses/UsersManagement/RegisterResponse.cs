#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Nemera.Presentation.Web.Common.Responses.UsersManagement;

/// <summary>
/// Represents the response model for a successful registration.
/// </summary>
/// <param name="Username">The username of the registered user.</param>
/// <param name="TotpSecret">The TOTP (Time-Based One-Time Password) secret used for two-factor authentication.</param>
[DebuggerDisplay("Username: {Username}")]
public record RegisterResponse(
    string? Username,
    string? TotpSecret
);
