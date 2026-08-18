#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Responses.Authentication;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Responses.Authentication;

/// <summary>
/// Contains unit tests for the <see cref="RecoverPasswordResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class RecoverPasswordResponseTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void RoundTrip_WhenSerializingRecoverPasswordResponse_ShouldPreserveValues()
    {
        // Arrange
        RecoverPasswordResponse expected = new(true);

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        RecoverPasswordResponse? actual = JsonSerializer.Deserialize<RecoverPasswordResponse>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void RoundTrip_WhenSerializingFalseRecoverPasswordResponse_ShouldPreserveFalse()
    {
        // Arrange
        RecoverPasswordResponse expected = new(false);

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        RecoverPasswordResponse? actual = JsonSerializer.Deserialize<RecoverPasswordResponse>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.False(actual.IsPasswordReset);
    }

    [Fact]
    public void Equality_WhenTwoInstancesHaveSameValues_ShouldBeEqual()
    {
        // Arrange
        RecoverPasswordResponse first = new(true);
        RecoverPasswordResponse second = new(true);

        // Act
        bool areEqual = first == second;

        // Assert
        Assert.True(areEqual);
    }
}
