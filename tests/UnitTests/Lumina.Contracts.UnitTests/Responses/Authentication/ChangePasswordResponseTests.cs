#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Responses.Authentication;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Responses.Authentication;

/// <summary>
/// Contains unit tests for the <see cref="ChangePasswordResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ChangePasswordResponseTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void RoundTrip_WhenSerializingChangePasswordResponse_ShouldPreserveValues()
    {
        // Arrange
        ChangePasswordResponse expected = new(true);

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        ChangePasswordResponse? actual = JsonSerializer.Deserialize<ChangePasswordResponse>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void RoundTrip_WhenSerializingFalseChangePasswordResponse_ShouldPreserveFalse()
    {
        // Arrange
        ChangePasswordResponse expected = new(false);

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        ChangePasswordResponse? actual = JsonSerializer.Deserialize<ChangePasswordResponse>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.False(actual.IsPasswordChanged);
    }

    [Fact]
    public void Equality_WhenTwoInstancesHaveSameValues_ShouldBeEqual()
    {
        // Arrange
        ChangePasswordResponse first = new(true);
        ChangePasswordResponse second = new(true);

        // Act
        bool areEqual = first == second;

        // Assert
        Assert.True(areEqual);
    }
}
