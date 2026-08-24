#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Responses.Authentication;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Responses.Authentication;

/// <summary>
/// Fixture class for the <see cref="LoginResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class LoginResponseFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="LoginResponse"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the authenticated user.</param>
    /// <param name="username">Optional. The username of the authenticated user.</param>
    /// <param name="token">Optional. The authentication token of the user.</param>
    /// <param name="usesTotp">Optional. Whether the user uses two-factor authentication.</param>
    /// <returns>The created <see cref="LoginResponse"/>.</returns>
    public LoginResponse Create(
        Guid? id = null, 
        string? username = null, 
        string? token = null, 
        bool? usesTotp = null)
    {
        return new LoginResponse(
            id ?? Guid.NewGuid(),
            username ?? _faker.Internet.UserName(),
            token ?? _faker.Random.Hexadecimal(32),
            usesTotp ?? _faker.Random.Bool());
    }

    /// <summary>
    /// Creates a list of <see cref="LoginResponse"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<LoginResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
