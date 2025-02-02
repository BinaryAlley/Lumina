#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Time;
using Lumina.Infrastructure.Common.Models.Configuration;
using Lumina.Infrastructure.Core.Authentication;
using Lumina.Infrastructure.UnitTests.Core.Authentication.Fixtures;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.Authentication;

/// <summary>
/// Contains unit tests for the <see cref="JwtTokenGenerator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class JwtTokenGeneratorTests
{
    private readonly IDateTimeProvider _mockDateTimeProvider;
    private readonly JwtSettingsModel _jwtSettings;
    private readonly JwtTokenGenerator _sut;
    private readonly DateTime _fixedUtcNow;

    /// <summary>
    /// Initializes a new instance of the <see cref="JwtTokenGeneratorTests"/> class.
    /// </summary>
    public JwtTokenGeneratorTests()
    {
        _mockDateTimeProvider = Substitute.For<IDateTimeProvider>();
        _jwtSettings = JwtTokenGeneratorFixture.CreateJwtSettings();
        _fixedUtcNow = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        _mockDateTimeProvider.UtcNow.Returns(_fixedUtcNow);

        IOptions<JwtSettingsModel> options = Substitute.For<IOptions<JwtSettingsModel>>();
        options.Value.Returns(_jwtSettings);

        _sut = new JwtTokenGenerator(_mockDateTimeProvider, options);
    }

    [Fact]
    public void GenerateToken_WhenCalled_ShouldCreateValidJwtToken()
    {
        // Arrange
        (string userId, string username) = JwtTokenGeneratorFixture.CreateUserCredentials();
        JwtSecurityTokenHandler tokenHandler = new();

        // Act
        string token = _sut.GenerateToken(userId, username);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);

        JwtSecurityToken jwtToken = tokenHandler.ReadJwtToken(token);

        // verify token properties
        Assert.Equal(_jwtSettings.Issuer, jwtToken.Issuer);
        Assert.Single(jwtToken.Audiences, _jwtSettings.Audience);
        Assert.Equal(_fixedUtcNow.AddMinutes(_jwtSettings.ExpiryMinutes), jwtToken.ValidTo);

        // verify claims
        Assert.Contains(jwtToken.Claims, c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == userId);
        Assert.Contains(jwtToken.Claims, c => c.Type == JwtRegisteredClaimNames.UniqueName && c.Value == username);
        Assert.Contains(jwtToken.Claims, c => c.Type == JwtRegisteredClaimNames.Jti && c.Value == userId);

        // verify the token is actually valid
        TokenValidationParameters validationParameters = new()
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)),
            ValidateIssuer = true,
            ValidIssuer = _jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = _jwtSettings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            // override the current time to match our fixed test time
            LifetimeValidator = (notBefore, expires, token, parameters) => expires > _fixedUtcNow
        };

        ClaimsPrincipal principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

        Assert.NotNull(principal);
        Assert.NotNull(validatedToken);
    }

    [Fact]
    public void GenerateToken_WhenCalled_ShouldUseProvidedDateTimeForExpiration()
    {
        // Arrange
        (string userId, string username) = JwtTokenGeneratorFixture.CreateUserCredentials();
        JwtSecurityTokenHandler tokenHandler = new();

        // Act
        string token = _sut.GenerateToken(userId, username);
        JwtSecurityToken jwtToken = tokenHandler.ReadJwtToken(token);

        // Assert
        Assert.Equal(_fixedUtcNow.AddMinutes(_jwtSettings.ExpiryMinutes), jwtToken.ValidTo);
        _ = _mockDateTimeProvider.Received(1).UtcNow;
    }

    [Fact]
    public void GenerateToken_WhenCalledWithDifferentUsers_ShouldCreateUniqueTokens()
    {
        // Arrange
        (string userId1, string username1) = JwtTokenGeneratorFixture.CreateUserCredentials();
        (string userId2, string username2) = JwtTokenGeneratorFixture.CreateUserCredentials();

        // Act
        string token1 = _sut.GenerateToken(userId1, username1);
        string token2 = _sut.GenerateToken(userId2, username2);

        // Assert
        Assert.NotEqual(token1, token2);
    }

    [Fact]
    public void GenerateToken_WhenCalledMultipleTimesForSameUser_ShouldCreateIdenticalTokens()
    {
        // Arrange
        (string userId, string username) = JwtTokenGeneratorFixture.CreateUserCredentials();

        // Act
        string token1 = _sut.GenerateToken(userId, username);
        string token2 = _sut.GenerateToken(userId, username);

        // Assert
        Assert.Equal(token1, token2);
    }
}
