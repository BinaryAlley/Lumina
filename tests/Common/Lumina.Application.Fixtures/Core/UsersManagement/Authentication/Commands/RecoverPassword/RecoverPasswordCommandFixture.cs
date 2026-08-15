#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.UsersManagement.Authentication.Commands.RecoverPassword;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.UsersManagement.Authentication.Commands.RecoverPassword;

/// <summary>
/// Fixture class for the <see cref="RecoverPasswordCommand"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RecoverPasswordCommandFixture
{
    /// <summary>
    /// Creates a random valid command to recover a password.
    /// </summary>
    /// <param name="username">Optional. The username.</param>
    /// <param name="totpCode">Optional. The TOTP code.</param>
    /// <returns>The created command.</returns>
    public RecoverPasswordCommand Create(string? username = null, string? totpCode = null)
    {
        return new Faker<RecoverPasswordCommand>()
            .CustomInstantiator(f => new RecoverPasswordCommand(
                default!,
                default!
            ))
            .RuleFor(x => x.Username, f => username ?? f.Person.UserName)
            .RuleFor(x => x.TotpCode, f => totpCode ?? f.Random.Replace("######"));
    }

    /// <summary>
    /// Creates a list of <see cref="RecoverPasswordCommand"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<RecoverPasswordCommand> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
