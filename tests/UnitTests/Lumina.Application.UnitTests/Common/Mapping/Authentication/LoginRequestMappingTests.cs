#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.Authentication;
using Lumina.Application.Core.UsersManagement.Authentication.Queries.LoginUser;
using Lumina.Application.UnitTests.Core.UsersManagement.Authentication.Queries.LoginUser.Fixtures;
using Lumina.Contracts.Requests.Authentication;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Authentication;

/// <summary>
/// Contains unit tests for the <see cref="LoginRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LoginRequestMappingTests
{
    private readonly LoginRequestFixture _fixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginRequestMappingTests"/> class.
    /// </summary>
    public LoginRequestMappingTests()
    {
        _fixture = new();
    }

    [Fact]
    public void ToQuery_WhenMappingRequest_ShouldMapCorrectly()
    {
        // Arrange
        LoginRequest request = _fixture.CreateLoginRequest();

        // Act
        LoginUserQuery result = request.ToQuery();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Username, result.Username);
        Assert.Equal(request.Password, result.Password);
        Assert.Equal(request.TotpCode, result.TotpCode);
    }

    [Fact]
    public void ToQuery_WhenMappingRequestWithNullValues_ShouldMapCorrectly()
    {
        // Arrange
        LoginRequest request = new(null, null);

        // Act
        LoginUserQuery result = request.ToQuery();

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Username);
        Assert.Null(result.Password);
        Assert.Null(result.TotpCode);
    }

    [Theory]
    [InlineData("user1", "pass123", "123456")]
    [InlineData("", "", "")]
    [InlineData("testUser", null, null)]
    [InlineData("admin", "admin123", null)]
    public void ToQuery_WhenMappingRequestWithSpecificValues_ShouldMapCorrectly(
        string? username,
        string? password,
        string? totpCode)
    {
        // Arrange
        LoginRequest request = new(username, password, totpCode);

        // Act
        LoginUserQuery result = request.ToQuery();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(username, result.Username);
        Assert.Equal(password, result.Password);
        Assert.Equal(totpCode, result.TotpCode);
    }
}
