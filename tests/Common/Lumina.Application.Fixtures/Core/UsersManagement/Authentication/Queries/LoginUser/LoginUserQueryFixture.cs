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
    /// <param name="includeTotpCode">Whether to include a TOTP code in the query.</param>
    /// <returns>The created query.</returns>
    public LoginUserQuery Create(
        string? username = null,
        string? password = null,
        bool includeTotpCode = false)
    {
        string resolvedPassword = password ?? "Abcd123$";
        Faker<LoginUserQuery> faker = new Faker<LoginUserQuery>()
            .CustomInstantiator(f => new LoginUserQuery(
                default!,
                default!,
                default
            ))
            .RuleFor(x => x.Username, f => username ?? f.Person.UserName)
            .RuleFor(x => x.Password, resolvedPassword);

        if (includeTotpCode)
            faker.RuleFor(x => x.TotpCode, f => f.Random.Replace("######")); // generates 6 random digits
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
