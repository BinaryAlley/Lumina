#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.Maintenance.ApplicationSetup.Commands.SetupApplication;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.Maintenance.ApplicationSetup.Commands.SetupApplication;

/// <summary>
/// Fixture class for the <see cref="SetupApplicationCommand"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SetupApplicationCommandFixture
{
    /// <summary>
    /// Creates a random valid command for the application setup.
    /// </summary>
    /// <param name="username">Optional. The admin username.</param>
    /// <param name="password">Optional. The admin password.</param>
    /// <param name="passwordConfirm">Optional. The admin password confirmation.</param>
    /// <param name="use2fa">Whether to use two-factor authentication.</param>
    /// <returns>The created command.</returns>
    public SetupApplicationCommand Create(
        string? username = null,
        string? password = null,
        string? passwordConfirm = null,
        bool use2fa = true)
    {
        string resolvedPassword = password ?? "Abcd123$";
        return new Faker<SetupApplicationCommand>()
            .CustomInstantiator(f => new SetupApplicationCommand(
                default!,
                default!,
                default!,
                true
            ))
            .RuleFor(x => x.Username, f => username ?? f.Internet.UserName())
            .RuleFor(x => x.Password, resolvedPassword)
            .RuleFor(x => x.PasswordConfirm, passwordConfirm ?? resolvedPassword)
            .RuleFor(x => x.Use2fa, use2fa)
            .Generate();
    }

    /// <summary>
    /// Creates a list of <see cref="SetupApplicationCommand"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<SetupApplicationCommand> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
