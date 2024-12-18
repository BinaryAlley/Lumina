#region ========================================================================= USING =====================================================================================
using System;
#endregion

namespace Lumina.Presentation.Web.Common.Models.UsersManagement;

/// <summary>
/// Represents a model for a user.
/// </summary>
/// <param name="Id">The Id of the user.</param>
/// <param name="Username">The username of the user.</param>
public record UserModel(
    Guid Id,
    string Username
);
