#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Requests.Authentication;
using Lumina.Contracts.Requests.Authentication;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Requests.Authentication;

/// <summary>
/// Contains unit tests for the <see cref="LoginRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class LoginRequestTests
{
    private readonly LoginRequestFixture _loginRequestFixture = new();

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Create_WhenCalled_ShouldReturnValidLoginRequest()
    {
        // Act
        LoginRequest sut = _loginRequestFixture.Create();

        // Assert
        Assert.NotNull(sut);
        Assert.False(string.IsNullOrWhiteSpace(sut.Username));
        Assert.False(string.IsNullOrWhiteSpace(sut.Password));
    }

    [Fact]
    public void RoundTrip_WhenSerializingLoginRequest_ShouldPreserveValues()
    {
        // Arrange
        LoginRequest expected = _loginRequestFixture.Create(totpCode: "123456");

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        LoginRequest? actual = JsonSerializer.Deserialize<LoginRequest>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Equality_WhenTwoInstancesHaveSameValues_ShouldBeEqual()
    {
        // Arrange
        LoginRequest first = new("user1", "pass1", null);
        LoginRequest second = new("user1", "pass1", null);

        // Act
        bool areEqual = first == second;

        // Assert
        Assert.True(areEqual);
    }

    [Fact]
    public void Deconstruct_WhenCalled_ShouldReturnAllProperties()
    {
        // Arrange
        LoginRequest sut = _loginRequestFixture.Create(username: "user1", password: "pass1", totpCode: "123456");

        // Act
        (string? username, string? password, string? totpCode) = sut;

        // Assert
        Assert.Equal(sut.Username, username);
        Assert.Equal(sut.Password, password);
        Assert.Equal(sut.TotpCode, totpCode);
    }
}
