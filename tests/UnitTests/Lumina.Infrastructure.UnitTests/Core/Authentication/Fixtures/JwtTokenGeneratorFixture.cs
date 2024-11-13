#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Infrastructure.Common.Models.Configuration;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.Authentication.Fixtures;

/// <summary>
/// Fixture class for JWT token generation tests.
/// </summary>
[ExcludeFromCodeCoverage]
public class JwtTokenGeneratorFixture
{
    /// <summary>
    /// Creates a valid JWT settings model for testing.
    /// </summary>
    /// <returns>The created settings model.</returns>
    public static JwtSettingsModel CreateJwtSettings()
    {
        return new Faker<JwtSettingsModel>()
            .CustomInstantiator(f => new JwtSettingsModel
            {
                SecretKey = "this-is-a-very-long-secret-key-for-testing-jwt-tokens",
                ExpiryMinutes = 30,
                Issuer = "test-issuer",
                Audience = "test-audience"
            })
            .Generate();
    }

    /// <summary>
    /// Creates test user credentials.
    /// </summary>
    /// <returns>A tuple containing the user ID and username.</returns>
    public static (string Id, string Username) CreateUserCredentials()
    {
        return (Guid.NewGuid().ToString(), new Faker().Person.UserName);
    }
}
