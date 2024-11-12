#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.UsersManagement.Authentication.Commands.ChangePassword;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.UsersManagement.Authentication.Commands.ChangePassword.Fixtures;

/// <summary>
/// Fixture class for the <see cref="ChangePasswordCommand"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ChangePasswordCommandFixture
{
    /// <summary>
    /// Creates a random valid command for changing a password.
    /// </summary>
    /// <returns>The created command.</returns>
    public static ChangePasswordCommand CreateChangePasswordCommand()
    {
        string currentPassword = "Abcd123$";
        string newPassword = "Wxyz789#";
        return new Faker<ChangePasswordCommand>()
            .CustomInstantiator(f => new ChangePasswordCommand(
                default!,
                default!,
                default!,
                default!
            ))
            .RuleFor(x => x.Username, f => f.Person.UserName)
            .RuleFor(x => x.CurrentPassword, currentPassword)
            .RuleFor(x => x.NewPassword, newPassword)
            .RuleFor(x => x.NewPasswordConfirm, newPassword);
    }
}
