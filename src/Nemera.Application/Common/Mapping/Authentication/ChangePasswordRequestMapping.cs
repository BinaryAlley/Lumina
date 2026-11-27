#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.UsersManagement.Authentication.Commands.ChangePassword;
using Lumina.Contracts.Requests.Authentication;
#endregion

namespace Lumina.Application.Common.Mapping.Authentication;

/// <summary>
/// Extension methods for converting <see cref="ChangePasswordRequest"/>.
/// </summary>
public static class ChangePasswordRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="ChangePasswordCommand"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted command.</returns>
    public static ChangePasswordCommand ToCommand(this ChangePasswordRequest request)
    {
        return new ChangePasswordCommand(
            request.Username,
            request.CurrentPassword,
            request.NewPassword,
            request.NewPasswordConfirm
        );
    }
}
