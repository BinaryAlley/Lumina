#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Models.UsersManagement;

/// <summary>
/// Represents the response model for a user authentication.
/// </summary>
/// <param name="Id">The Id of the logged in user.</param>
/// <param name="Username">The username of the logged in user.</param>
/// <param name="Token">The JWT authentication token, <see langword="null"/> if authentication failed or TOTP is required.</param>
/// <param name="UsesTotp">Indicates whether the user has Two-Factor Authentication enabled, or not.</param>
[DebuggerDisplay("Username: {Username}")]
public record LoginResponseModel(
    Guid Id,
    string? Username,
    string? Token,
    bool UsesTotp
);
