#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Responses.UsersManagement;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Responses.UsersManagement;

/// <summary>
/// Contains unit tests for the <see cref="InitializationResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class InitializationResponseTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void RoundTrip_WhenSerializingInitializationResponse_ShouldPreserveValues()
    {
        // Arrange
        InitializationResponse expected = new(true);

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        InitializationResponse? actual = JsonSerializer.Deserialize<InitializationResponse>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void RoundTrip_WhenSerializingFalseInitializationResponse_ShouldPreserveFalse()
    {
        // Arrange
        InitializationResponse expected = new(false);

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        InitializationResponse? actual = JsonSerializer.Deserialize<InitializationResponse>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.False(actual.IsInitialized);
    }

    [Fact]
    public void Equality_WhenTwoInstancesHaveSameValues_ShouldBeEqual()
    {
        // Arrange
        InitializationResponse first = new(true);
        InitializationResponse second = new(true);

        // Act
        bool areEqual = first == second;

        // Assert
        Assert.True(areEqual);
    }
}
