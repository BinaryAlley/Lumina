#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Requests.Authentication;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.Authentication;

/// <summary>
/// Fixture class for the <see cref="LoginRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class LoginRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="LoginRequest"/>.
    /// </summary>
    /// <param name="username">Optional. The username for login.</param>
    /// <param name="password">Optional. The password for login.</param>
    /// <param name="totpCode">Optional. The TOTP code for two-factor authentication.</param>
    /// <param name="includeUsername">Whether the username should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includePassword">Whether the password should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeTotpCode">Whether the TOTP code should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="LoginRequest"/>.</returns>
    public LoginRequest Create(
        string? username = null,
        string? password = null,
        string? totpCode = null,
        bool includeUsername = true,
        bool includePassword = true,
        bool includeTotpCode = true)
    {
        return new LoginRequest(
            includeUsername ? (username ?? _faker.Internet.UserName()) : null,
            includePassword ? (password ?? _faker.Internet.Password()) : null,
            includeTotpCode ? (totpCode ?? _faker.Random.Number(100000, 999999).ToString()) : null
        );
    }

    /// <summary>
    /// Creates a list of <see cref="LoginRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<LoginRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
