#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.Requests.UsersManagement.Authentication;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.UsersManagement.Authentication;

/// <summary>
/// Fixture class for generating <see cref="LoginViewRequest"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class LoginViewRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="LoginViewRequest"/> instance with randomized test data.
    /// </summary>
    /// <param name="returnUrl">Optional URL to return to, after login.</param>
    /// <returns>A configured <see cref="LoginViewRequest"/> instance.</returns>
    public LoginViewRequest Create(
        string? returnUrl = null)
    {
        return new LoginViewRequest(
            ReturnUrl: returnUrl ?? $"/{_faker.Random.AlphaNumeric(8)}"
        );
    }

    /// <summary>
    /// Creates multiple <see cref="LoginViewRequest"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="LoginViewRequest"/> instances.</returns>
    public List<LoginViewRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
