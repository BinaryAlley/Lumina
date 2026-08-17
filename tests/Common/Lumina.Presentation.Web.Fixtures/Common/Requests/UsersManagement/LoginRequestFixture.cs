#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.Requests.UsersManagement;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.UsersManagement;

/// <summary>
/// Fixture class for generating <see cref="LoginRequest"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class LoginRequestFixture
{
    /// <summary>
    /// Creates a new <see cref="LoginRequest"/> instance with randomized test data.
    /// </summary>
    /// <param name="username">Optional username.</param>
    /// <param name="password">Optional password.</param>
    /// <param name="totpCode">Optional TOTP code.</param>
    /// <param name="returnUrl">Optional URL to return to, after login.</param>
    /// <returns>A configured <see cref="LoginRequest"/> instance.</returns>
    public LoginRequest Create(string? username = null, string? password = null, string? totpCode = null, string? returnUrl = null)
    {
        Faker faker = new();
        return new LoginRequest(
            Username: username ?? faker.Internet.UserName(),
            Password: password ?? faker.Internet.Password(12),
            TotpCode: totpCode,
            ReturnUrl: returnUrl
        );
    }

    /// <summary>
    /// Creates multiple <see cref="LoginRequest"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="LoginRequest"/> instances.</returns>
    public List<LoginRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
