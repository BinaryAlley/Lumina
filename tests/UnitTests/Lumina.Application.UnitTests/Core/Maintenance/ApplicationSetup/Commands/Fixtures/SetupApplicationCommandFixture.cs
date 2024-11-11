#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.Maintenance.ApplicationSetup.Commands;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.Maintenance.ApplicationSetup.Commands.Fixtures;

/// <summary>
/// Fixture class for the <see cref="SetupApplicationCommand"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SetupApplicationCommandFixture
{
    /// <summary>
    /// Creates a random valid command for the application setup.
    /// </summary>
    /// <returns>The created command.</returns>
    public static SetupApplicationCommand CreateSetupApplicationCommand()
    {
        string password = "Abcd123$";
        return new Faker<SetupApplicationCommand>()
            .CustomInstantiator(f => new SetupApplicationCommand(
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
