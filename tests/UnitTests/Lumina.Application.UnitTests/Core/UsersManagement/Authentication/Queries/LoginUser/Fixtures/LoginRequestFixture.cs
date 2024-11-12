#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Requests.Authentication;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Core.UsersManagement.Authentication.Queries.LoginUser.Fixtures;

/// <summary>
/// Fixture class for the <see cref="LoginRequest"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LoginRequestFixture
{
    private readonly Faker _faker;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginRequestFixture"/> class.
    /// </summary>
    public LoginRequestFixture()
    {
        _faker = new Faker();
    }

    /// <summary>
    /// Creates a random valid <see cref="LoginRequest"/>.
    /// </summary>
    /// <returns>The created <see cref="LoginRequest"/>.</returns>
    public LoginRequest CreateLoginRequest(
        string? username = null,
        string? password = null,
        string? totpCode = null)
    {
        return new LoginRequest(
            username ?? _faker.Internet.UserName(),
            password ?? _faker.Internet.Password(),
            totpCode ?? _faker.Random.Number(100000, 999999).ToString()
        );
    }

    /// <summary>
    /// Creates a list of <see cref="LoginRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<LoginRequest> CreateMany(int count = 3)
    {
        return Enumerable.Range(0, count)
            .Select(_ => CreateLoginRequest())
            .ToList();
    }
}
