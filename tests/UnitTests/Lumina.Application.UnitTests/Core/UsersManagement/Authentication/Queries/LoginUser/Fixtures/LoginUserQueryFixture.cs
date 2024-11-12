#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.UsersManagement.Authentication.Queries.LoginUser;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.UsersManagement.Authentication.Queries.LoginUser.Fixtures;

/// <summary>
/// Fixture class for the <see cref="LoginUserQuery"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LoginUserQueryFixture
{
    /// <summary>
    /// Creates a random valid query for user login.
    /// </summary>
    /// <param name="includeTotpCode">Whether to include a TOTP code in the query.</param>
    /// <returns>The created query.</returns>
    public static LoginUserQuery CreateLoginQuery(bool includeTotpCode = false)
    {
        string password = "Abcd123$";
        Faker<LoginUserQuery> faker = new Faker<LoginUserQuery>()
            .CustomInstantiator(f => new LoginUserQuery(
                default!,
                default!,
                default
            ))
            .RuleFor(x => x.Username, f => f.Person.UserName)
            .RuleFor(x => x.Password, password);

        if (includeTotpCode)
            faker.RuleFor(x => x.TotpCode, f => f.Random.Replace("######")); // generates 6 random digits
        return faker;
    }
}
