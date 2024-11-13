namespace Lumina.Presentation.Web.Common.Models.UsersManagement;

/// <summary>
/// Represents the request model for account password change.
/// </summary>
/// <param name="Username">The username of the account.</param>
/// <param name="CurrentPassword">The current password of the account.</param>
/// <param name="NewPassword">The new password of the account.</param>
/// <param name="NewPasswordConfirm">The confirmation of the new password of the account.</param>
public record ChangePasswordRequestModel(
    string? Username,
    string? CurrentPassword,    
    string? NewPassword,
    string? NewPasswordConfirm
);
