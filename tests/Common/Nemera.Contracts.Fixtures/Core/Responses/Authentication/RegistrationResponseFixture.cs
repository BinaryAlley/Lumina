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
/// Fixture class for the <see cref="RegistrationResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class RegistrationResponseFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="RegistrationResponse"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the registered user.</param>
    /// <param name="username">Optional. The username of the registered user.</param>
    /// <param name="totpSecret">Optional. The TOTP secret used for two-factor authentication.</param>
    /// <returns>The created <see cref="RegistrationResponse"/>.</returns>
    public RegistrationResponse Create(
        Guid? id = null,
        string? username = null,
        string? totpSecret = null)
    {
        return new RegistrationResponse(
            id ?? Guid.NewGuid(),
            username ?? _faker.Internet.UserName(),
            totpSecret ?? _faker.Random.AlphaNumeric(16)
        );
    }

    /// <summary>
    /// Creates a list of <see cref="RegistrationResponse"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<RegistrationResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
