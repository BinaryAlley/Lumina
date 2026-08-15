#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.UsersManagement.Authentication.Commands.RegisterUser;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.UsersManagement.Authentication.Commands.RegisterUser;

/// <summary>
/// Fixture class for the <see cref="RegisterUserCommand"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RegisterUserCommandFixture
{
    /// <summary>
    /// Creates a random valid command for a new account registration.
    /// </summary>
    /// <param name="username">Optional. The username.</param>
    /// <param name="password">Optional. The password.</param>
    /// <param name="passwordConfirm">Optional. The password confirmation.</param>
    /// <param name="use2fa">Whether to use two-factor authentication.</param>
    /// <returns>The created command.</returns>
    public RegisterUserCommand Create(
        string? username = null,
        string? password = null,
        string? passwordConfirm = null,
        bool use2fa = true)
    {
        string resolvedPassword = password ?? "Abcd123$";
        return new Faker<RegisterUserCommand>()
            .CustomInstantiator(f => new RegisterUserCommand(
                default!,
                default!,
                default!,
                true
            ))
            .RuleFor(x => x.Username, f => username ?? f.Person.UserName)
            .RuleFor(x => x.Password, resolvedPassword)
            .RuleFor(x => x.PasswordConfirm, passwordConfirm ?? resolvedPassword)
            .RuleFor(x => x.Use2fa, use2fa);
    }

    /// <summary>
    /// Creates a list of <see cref="RegisterUserCommand"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<RegisterUserCommand> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
