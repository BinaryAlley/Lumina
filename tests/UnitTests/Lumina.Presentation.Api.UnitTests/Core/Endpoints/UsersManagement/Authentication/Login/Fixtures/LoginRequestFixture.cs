#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Requests.Authentication;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.UsersManagement.Authentication.Login.Fixtures;

/// <summary>
/// Fixture class for the <see cref="LoginRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class LoginRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a <see cref="LoginRequest"/> with default or random values.
    /// </summary>
    /// <param name="username">The username for login.</param>
    /// <param name="password">The password for login.</param>
    /// <param name="totpCode">The TOTP code for two-factor authentication.</param>
    /// <returns>The created <see cref="LoginRequest"/>.</returns>
    public LoginRequest Create(
        string? username = null,
        string? password = null,
        string? totpCode = null)
    {
        return new LoginRequest(
            Username: username ?? _faker.Internet.UserName(),
            Password: password ?? _faker.Internet.Password(),
            TotpCode: totpCode ?? _faker.Random.Number(100000, 999999).ToString()
        );
    }
}
