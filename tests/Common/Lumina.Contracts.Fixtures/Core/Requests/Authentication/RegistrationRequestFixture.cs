#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Requests.Authentication;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.Authentication;

/// <summary>
/// Fixture class for the <see cref="RegistrationRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class RegistrationRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="RegistrationRequest"/>.
    /// </summary>
    /// <param name="username">Optional. The username for registration.</param>
    /// <param name="password">Optional. The password for registration.</param>
    /// <param name="passwordConfirm">Optional. The password confirmation.</param>
    /// <param name="use2fa">Optional. Whether to use two-factor authentication.</param>
    /// <param name="includeUsername">Whether the username should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includePassword">Whether the password should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includePasswordConfirm">Whether the password confirmation should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="RegistrationRequest"/>.</returns>
    public RegistrationRequest Create(
        string? username = null,
        string? password = null,
        string? passwordConfirm = null,
        bool? use2fa = null,
        bool includeUsername = true,
        bool includePassword = true,
        bool includePasswordConfirm = true)
    {
        string generatedPassword = password ?? _faker.Internet.Password();
        return new RegistrationRequest(
            includeUsername ? (username ?? _faker.Internet.UserName()) : null,
            includePassword ? generatedPassword : null,
            includePasswordConfirm ? (passwordConfirm ?? generatedPassword) : null,
            use2fa ?? _faker.Random.Bool()
        );
    }

    /// <summary>
    /// Creates a list of <see cref="RegistrationRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<RegistrationRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
