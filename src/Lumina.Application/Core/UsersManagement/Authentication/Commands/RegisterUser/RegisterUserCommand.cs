#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Responses.Authentication;
using Mediator;
#endregion

namespace Lumina.Application.Core.UsersManagement.Authentication.Commands.RegisterUser;

/// <summary>
/// Command for registering a new account.
/// </summary>
/// <param name="Username">The username for the new account.</param>
/// <param name="Password">The password for the new account.</param>
/// <param name="PasswordConfirm">Confirmation of the password.</param>
/// <param name="Use2fa">Indicates whether to enable two-factor authentication for the new account.</param>
public record RegisterUserCommand(
    string? Username,
    string? Password,
    string? PasswordConfirm,
    bool Use2fa
) : IRequest<ErrorOr<RegistrationResponse>>;
