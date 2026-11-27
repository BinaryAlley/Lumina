#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Infrastructure.Common.Models.DTO.Configuration;
using Lumina.Infrastructure.Fixtures.Common.Models.DTO.Configuration;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Infrastructure.Fixtures.Core.Authentication;

/// <summary>
/// Test-support class for JWT token generation tests.
/// </summary>
[ExcludeFromCodeCoverage]
public class JwtTokenGeneratorFixture
{
    /// <summary>
    /// Creates a valid JWT settings model for testing.
    /// </summary>
    /// <returns>The created settings model.</returns>
    public static JwtSettingsDto CreateJwtSettings()
    {
        return new JwtSettingsDtoFixture().Create();
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
