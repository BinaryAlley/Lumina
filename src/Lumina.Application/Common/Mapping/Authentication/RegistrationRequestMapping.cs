#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Maintenance.ApplicationSetup.Commands;
using Lumina.Application.Core.UsersManagement.Register;
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
    /// Converts <paramref name="request"/> to <see cref="RegisterCommand"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted command.</returns>
    public static RegisterCommand ToCommand(this RegistrationRequest request)
    {
        return new RegisterCommand(
            request.Username,
            request.Password,
            request.PasswordConfirm,
            request.Use2fa
        );
    }
}
