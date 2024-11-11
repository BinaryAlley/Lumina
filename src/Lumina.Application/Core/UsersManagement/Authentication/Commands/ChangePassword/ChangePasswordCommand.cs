#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Responses.Authentication;
using Mediator;
#endregion

namespace Lumina.Application.Core.UsersManagement.Authentication.Commands.ChangePassword;

/// <summary>
/// Represents the request model for account password change.
/// </summary>
/// <param name="Username">The username of the account for which to change the password.</param>
/// <param name="CurrentPassword">The current password of the account for which to change the password.</param>
/// <param name="NewPassword">The new password of the account for which to change the password.</param>
/// <param name="NewPasswordConfirm">The confirmation of the new password of the account for which to change the password.</param>
public record ChangePasswordCommand(
    string? Username,
    string? CurrentPassword,
    string? NewPassword,
    string? NewPasswordConfirm
) : IRequest<ErrorOr<ChangePasswordResponse>>;
