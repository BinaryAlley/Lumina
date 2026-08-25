#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.Requests.UsersManagement;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.UsersManagement;

/// <summary>
/// Fixture class for generating <see cref="RegisterRequest"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class RegisterRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="RegisterRequest"/> instance with randomized test data.
    /// </summary>
    /// <param name="username">Optional username.</param>
    /// <param name="password">Optional password.</param>
    /// <param name="passwordConfirm">Optional password confirmation.</param>
    /// <param name="registrationType">Optional registration type, e.g. "Admin" or "User".</param>
    /// <param name="use2fa">Whether two-factor authentication is enabled for the account.</param>
    /// <returns>A configured <see cref="RegisterRequest"/> instance.</returns>
    public RegisterRequest Create(
        string? username = null, 
        string? password = null, 
        string? passwordConfirm = null, 
        string? registrationType = "User", 
        bool use2fa = true)
    {
        string generatedPassword = password ?? _faker.Internet.Password(12);
        return new RegisterRequest(
            Username: username ?? _faker.Internet.UserName(),
            Password: generatedPassword,
            PasswordConfirm: passwordConfirm ?? generatedPassword,
            RegistrationType: registrationType,
            Use2fa: use2fa
        );
    }

    /// <summary>
    /// Creates multiple <see cref="RegisterRequest"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="RegisterRequest"/> instances.</returns>
    public List<RegisterRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
