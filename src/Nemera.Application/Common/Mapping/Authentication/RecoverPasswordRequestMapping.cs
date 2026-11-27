#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.UsersManagement.Authentication.Commands.RecoverPassword;
using Lumina.Contracts.Requests.Authentication;
#endregion

namespace Lumina.Application.Common.Mapping.Authentication;

/// <summary>
/// Extension methods for converting <see cref="RecoverPasswordRequest"/>.
/// </summary>
public static class RecoverPasswordRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="RecoverPasswordCommand"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted command.</returns>
    public static RecoverPasswordCommand ToCommand(this RecoverPasswordRequest request)
    {
        return new RecoverPasswordCommand(
            request.Username,
            request.TotpCode
        );
    }
}
