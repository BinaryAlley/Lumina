#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.Authentication;
using Lumina.Application.Core.UsersManagement.Authentication.Queries.LoginUser;
using Lumina.Contracts.Fixtures.Core.Requests.Authentication;
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
    private readonly LoginRequestFixture _loginRequestFixture = new();

    [Fact]
    public void ToQuery_WhenMappingRequest_ShouldMapCorrectly()
    {
        // Arrange
        LoginRequest request = _loginRequestFixture.Create();

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
        LoginRequest request = _loginRequestFixture.Create(includeUsername: false, includePassword: false, includeTotpCode: false);

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
        LoginRequest request = _loginRequestFixture.Create(
            username,
            password,
            totpCode,
            includeUsername: username is not null,
            includePassword: password is not null,
            includeTotpCode: totpCode is not null);

        // Act
        LoginUserQuery result = request.ToQuery();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(username, result.Username);
        Assert.Equal(password, result.Password);
        Assert.Equal(totpCode, result.TotpCode);
    }
}
