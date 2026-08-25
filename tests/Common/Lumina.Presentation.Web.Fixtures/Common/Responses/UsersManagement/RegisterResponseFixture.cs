#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.Responses.UsersManagement;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Responses.UsersManagement;

/// <summary>
/// Fixture class for generating <see cref="RegisterResponse"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class RegisterResponseFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="RegisterResponse"/> instance with randomized test data.
    /// </summary>
    /// <param name="username">Optional username of the registered user.</param>
    /// <param name="totpSecret">Optional TOTP secret used for two-factor authentication.</param>
    /// <returns>A configured <see cref="RegisterResponse"/> instance.</returns>
    public RegisterResponse Create(
        string? username = null, 
        string? totpSecret = null)
    {
        return new RegisterResponse(
            Username: username ?? _faker.Internet.UserName(),
            TotpSecret: totpSecret ?? _faker.Random.AlphaNumeric(16)
        );
    }

    /// <summary>
    /// Creates multiple <see cref="RegisterResponse"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="RegisterResponse"/> instances.</returns>
    public List<RegisterResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
