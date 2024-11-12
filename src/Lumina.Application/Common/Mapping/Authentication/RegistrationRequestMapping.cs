#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Maintenance.ApplicationSetup.Commands.SetupApplication;
using Lumina.Application.Core.UsersManagement.Authentication.Commands.RegisterUser;
using Lumina.Contracts.Requests.Authentication;
#endregion

namespace Lumina.Application.Common.Mapping.Authentication;

/// <summary>
/// Extension methods for converting <see cref="RegistrationRequest"/>.
/// </summary>
public static class RegistrationRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="SetupApplicationCommand"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted command.</returns>
    public static SetupApplicationCommand ToSetupCommand(this RegistrationRequest request)
    {
        return new SetupApplicationCommand(
            request.Username,
            request.Password,
            request.PasswordConfirm,
            request.Use2fa
        );
    }

    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="RegisterUserCommand"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted command.</returns>
    public static RegisterUserCommand ToCommand(this RegistrationRequest request)
    {
        return new RegisterUserCommand(
            request.Username,
            request.Password,
            request.PasswordConfirm,
            request.Use2fa
        );
    }
}
