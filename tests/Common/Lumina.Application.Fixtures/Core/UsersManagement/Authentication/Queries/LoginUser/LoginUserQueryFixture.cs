#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.UsersManagement.Authentication.Queries.LoginUser;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.UsersManagement.Authentication.Queries.LoginUser;

/// <summary>
/// Fixture class for the <see cref="LoginUserQuery"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LoginUserQueryFixture
{
    /// <summary>
    /// Creates a random valid query for user login.
    /// </summary>
    /// <param name="username">Optional. The username.</param>
    /// <param name="password">Optional. The password.</param>
    /// <param name="includeTotpCode">Whether the TOTP code should be included, or forced to <see langword="null"/>.</param>
    /// <param name="totpCode">Optional. The TOTP code. When provided, the TOTP code is included even if <paramref name="includeTotpCode"/> is <see langword="false"/>.</param>
    /// <returns>The created query.</returns>
    public LoginUserQuery Create(
        string? username = null,
        string? password = null,
        bool includeTotpCode = false,
        string? totpCode = null)
    {
        string resolvedPassword = password ?? "Abcd123$";
        bool shouldIncludeTotpCode = includeTotpCode || totpCode is not null;
        Faker<LoginUserQuery> faker = new Faker<LoginUserQuery>()
            .CustomInstantiator(f => new LoginUserQuery(
                default!,
                default!,
                default
            ))
            .RuleFor(x => x.Username, f => username ?? f.Person.UserName)
            .RuleFor(x => x.Password, resolvedPassword);

        if (shouldIncludeTotpCode)
            faker.RuleFor(x => x.TotpCode, f => totpCode ?? f.Random.Replace("######")); // generates 6 random digits when no code is pinned
        return faker.Generate();
    }

    /// <summary>
    /// Creates a list of <see cref="LoginUserQuery"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<LoginUserQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
