#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.UsersManagement.Authentication.Commands.RecoverPassword;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.UsersManagement.Authentication.Commands.RecoverPassword.Fixtures;

/// <summary>
/// Fixture class for the <see cref="RecoverPasswordCommand"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RecoverPasswordCommandFixture
{
    /// <summary>
    /// Creates a random valid command for password recovery.
    /// </summary>
    /// <returns>The created command.</returns>
    public static RecoverPasswordCommand CreateRecoverPasswordCommand()
    {
        return new Faker<RecoverPasswordCommand>()
            .CustomInstantiator(f => new RecoverPasswordCommand(
                default!,
                default!
            ))
            .RuleFor(x => x.Username, f => f.Person.UserName)
            .RuleFor(x => x.TotpCode, f => f.Random.Replace("######")); // generates 6 random digits
    }
}
