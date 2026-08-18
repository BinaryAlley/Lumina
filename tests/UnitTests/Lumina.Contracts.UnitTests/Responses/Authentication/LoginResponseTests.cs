#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Responses.Authentication;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Responses.Authentication;

/// <summary>
/// Contains unit tests for the <see cref="LoginResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class LoginResponseTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void RoundTrip_WhenSerializingLoginResponse_ShouldPreserveValues()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        LoginResponse expected = new(id, "testUser", "jwt_token", true);

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        LoginResponse? actual = JsonSerializer.Deserialize<LoginResponse>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Equality_WhenTwoInstancesHaveSameValues_ShouldBeEqual()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        LoginResponse first = new(id, "testUser", "jwt_token", false);
        LoginResponse second = new(id, "testUser", "jwt_token", false);

        // Act
        bool areEqual = first == second;

        // Assert
        Assert.True(areEqual);
    }

    [Fact]
    public void Deconstruct_WhenCalled_ShouldReturnAllProperties()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        LoginResponse sut = new(id, "testUser", "jwt_token", true);

        // Act
        (Guid sutId, string username, string token, bool usesTotp) = sut;

        // Assert
        Assert.Equal(sut.Id, sutId);
        Assert.Equal(sut.Username, username);
        Assert.Equal(sut.Token, token);
        Assert.Equal(sut.UsesTotp, usesTotp);
    }
}
