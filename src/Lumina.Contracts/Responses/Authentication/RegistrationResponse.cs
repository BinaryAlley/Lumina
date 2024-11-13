#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.Authentication;

/// <summary>
/// Represents the response model for user registration.
/// </summary>
/// <param name="Id">The Id of the user.</param>
/// <param name="Username">The username of the user.</param>
/// <param name="TotpSecret">The TOTP (Time-Based One-Time Password) secret used for two-factor authentication.</param>
[DebuggerDisplay("Username: {Username}")]
public record RegistrationResponse(
    Guid Id,
    string Username,
    string? TotpSecret
);
