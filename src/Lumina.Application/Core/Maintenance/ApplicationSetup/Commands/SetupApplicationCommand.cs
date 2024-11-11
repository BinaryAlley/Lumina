#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Responses.Authentication;
using Mediator;
#endregion

namespace Lumina.Application.Core.Maintenance.ApplicationSetup.Commands;

/// <summary>
/// Command for the initial application setup.
/// </summary>
/// <param name="Username">The username for the Admin account.</param>
/// <param name="Password">The password for the Admin account.</param>
/// <param name="PasswordConfirm">Confirmation of the password.</param>
/// <param name="Use2fa">Indicates whether to enable two-factor authentication for the Admin account.</param>
public record SetupApplicationCommand(
    string? Username, 
    string? Password, 
    string? PasswordConfirm, 
    bool Use2fa
) : IRequest<ErrorOr<RegistrationResponse>>;
