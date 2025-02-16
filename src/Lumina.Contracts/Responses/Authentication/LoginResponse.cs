#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.Authentication;

/// <summary>
/// Represents the response model for user authentication.
/// </summary>
/// <param name="Id">The unique identifier of the user to authenticate.</param>
/// <param name="Username">The username of the user to authenticate.</param>
/// <param name="Token">The authentication token used for subsequent requests.</param>
/// <param name="UsesTotp">Whether the user uses 2FA authentication, or not.</param>
[DebuggerDisplay("Username: {Username}")]
public record LoginResponse(
    Guid Id,
    string Username,
    string Token,
    bool UsesTotp
);
