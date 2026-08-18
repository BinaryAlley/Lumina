#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Requests.Authentication;
using Lumina.Contracts.Requests.Authentication;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Requests.Authentication;

/// <summary>
/// Contains unit tests for the <see cref="RecoverPasswordRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class RecoverPasswordRequestTests
{
    private readonly RecoverPasswordRequestFixture _recoverPasswordRequestFixture = new();

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Create_WhenCalled_ShouldReturnValidRecoverPasswordRequest()
    {
        // Act
        RecoverPasswordRequest sut = _recoverPasswordRequestFixture.Create();

        // Assert
        Assert.NotNull(sut);
        Assert.False(string.IsNullOrWhiteSpace(sut.Username));
        Assert.False(string.IsNullOrWhiteSpace(sut.TotpCode));
    }

    [Fact]
    public void RoundTrip_WhenSerializingRecoverPasswordRequest_ShouldPreserveValues()
    {
        // Arrange
        RecoverPasswordRequest expected = _recoverPasswordRequestFixture.Create();

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        RecoverPasswordRequest? actual = JsonSerializer.Deserialize<RecoverPasswordRequest>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Deconstruct_WhenCalled_ShouldReturnAllProperties()
    {
        // Arrange
        RecoverPasswordRequest sut = _recoverPasswordRequestFixture.Create(username: "user1", totpCode: "123456");

        // Act
        (string? username, string? totpCode) = sut;

        // Assert
        Assert.Equal(sut.Username, username);
        Assert.Equal(sut.TotpCode, totpCode);
    }
}
