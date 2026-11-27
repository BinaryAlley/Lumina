#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Infrastructure.Common.Models.DTO.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography;
#endregion

namespace Lumina.Infrastructure.Fixtures.Common.Models.DTO.Configuration;

/// <summary>
/// Fixture class for the <see cref="JwtSettingsDto"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class JwtSettingsDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a valid <see cref="JwtSettingsDto"/>.
    /// </summary>
    /// <param name="secretKey">Optional. The secret key used to sign the JWT.</param>
    /// <param name="expiryMinutes">Optional. The duration (in minutes) for which the JWT is valid.</param>
    /// <param name="issuer">Optional. The issuer of the JWT.</param>
    /// <param name="audience">Optional. The intended audience for the JWT.</param>
    /// <returns>The created <see cref="JwtSettingsDto"/>.</returns>
    public JwtSettingsDto Create(
        string? secretKey = null,
        int? expiryMinutes = null,
        string? issuer = null,
        string? audience = null)
    {
        byte[] key = new byte[32];
        RandomNumberGenerator.Fill(key);

        return new JwtSettingsDto
        {
            SecretKey = secretKey ?? Convert.ToBase64String(key),
            ExpiryMinutes = expiryMinutes ?? _faker.Random.Int(1, 120),
            Issuer = issuer ?? _faker.Company.CompanyName(),
            Audience = audience ?? _faker.Company.CompanyName()
        };
    }

    /// <summary>
    /// Creates a list of <see cref="JwtSettingsDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="JwtSettingsDto"/> instances.</returns>
    public List<JwtSettingsDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
