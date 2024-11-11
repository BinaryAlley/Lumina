#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.UsersManagement.Authentication.Commands.RegisterUser;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.UsersManagement.Register.Fixture;

/// <summary>
/// Fixture class for the <see cref="RegisterUserCommand"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RegisterCommandFixture
{
    /// <summary>
    /// Creates a random valid command for a new account registration.
    /// </summary>
    /// <returns>The created command.</returns>
    public static RegisterUserCommand CreateRegisterCommand()
    {
        string password = "Abcd123$";
        return new Faker<RegisterUserCommand>()
            .CustomInstantiator(f => new RegisterUserCommand(
                default!,
                default!,
                default!,
                true
            ))
            .RuleFor(x => x.Username, f => f.Person.UserName)
            .RuleFor(x => x.Password, password)
            .RuleFor(x => x.PasswordConfirm, password);
    }
}
