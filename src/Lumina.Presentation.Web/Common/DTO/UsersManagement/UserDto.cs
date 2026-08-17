#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.UsersManagement;

/// <summary>
/// Data transfer object for a user.
/// </summary>
/// <param name="Id">The unique identifier of the user.</param>
/// <param name="Username">The username of the user.</param>
[DebuggerDisplay("Id: {Id}, Username: {Username}")]
public record UserDto(
    Guid Id,
    string Username
);
