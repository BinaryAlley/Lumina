#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.MediaContributors;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.DTO.MediaContributors;

/// <summary>
/// Contains unit tests for the <see cref="MediaContributorNameDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaContributorNameDtoTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void RoundTrip_WhenSerializingName_ShouldPreserveValues()
    {
        // Arrange
        MediaContributorNameDto expected = new("John Smith", "Johnathan Smith");

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        MediaContributorNameDto? actual = JsonSerializer.Deserialize<MediaContributorNameDto>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Equality_WhenTwoInstancesHaveSameValues_ShouldBeEqual()
    {
        // Arrange
        MediaContributorNameDto first = new("John Smith", null);
        MediaContributorNameDto second = new("John Smith", null);

        // Act
        bool areEqual = first == second;

        // Assert
        Assert.True(areEqual);
    }
}
