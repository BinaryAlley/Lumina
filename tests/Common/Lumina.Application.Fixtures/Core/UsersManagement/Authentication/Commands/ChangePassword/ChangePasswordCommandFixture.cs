#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.UsersManagement.Authentication.Commands.ChangePassword;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.UsersManagement.Authentication.Commands.ChangePassword;

/// <summary>
/// Fixture class for the <see cref="ChangePasswordCommand"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ChangePasswordCommandFixture
{
    /// <summary>
    /// Creates a random valid command to change a password.
    /// </summary>
    /// <param name="username">Optional. The username.</param>
    /// <param name="currentPassword">Optional. The current password.</param>
    /// <param name="newPassword">Optional. The new password.</param>
    /// <param name="newPasswordConfirm">Optional. The new password confirmation.</param>
    /// <returns>The created command.</returns>
    public ChangePasswordCommand Create(
        string? username = null,
        string? currentPassword = null,
        string? newPassword = null,
        string? newPasswordConfirm = null)
    {
        string resolvedCurrentPassword = currentPassword ?? "CurrentP@ss123";
        string resolvedNewPassword = newPassword ?? "Abcd123$";
        return new Faker<ChangePasswordCommand>()
            .CustomInstantiator(f => new ChangePasswordCommand(
                default!,
                default!,
                default!,
                default!
            ))
            .RuleFor(x => x.Username, f => username ?? f.Person.UserName)
            .RuleFor(x => x.CurrentPassword, resolvedCurrentPassword)
            .RuleFor(x => x.NewPassword, resolvedNewPassword)
            .RuleFor(x => x.NewPasswordConfirm, newPasswordConfirm ?? resolvedNewPassword);
    }

    /// <summary>
    /// Creates a list of <see cref="ChangePasswordCommand"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<ChangePasswordCommand> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
