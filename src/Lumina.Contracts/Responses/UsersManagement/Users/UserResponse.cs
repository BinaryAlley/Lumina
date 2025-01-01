#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.UsersManagement.Users;

/// <summary>
/// Represents a user response.
/// </summary>
/// <param name="Id">The id of the user.</param>
/// <param name="Username">The username of the user.</param>
/// <param name="CreatedOnUtc">The date and time when the user was created.</param>
/// <param name="UpdatedOnUtc">The optional date and time when the user was updated.</param>
[DebuggerDisplay("Username: {Username}")]
public record UserResponse(
    Guid Id,
    string Username,
    DateTime CreatedOnUtc,
    DateTime? UpdatedOnUtc
);
