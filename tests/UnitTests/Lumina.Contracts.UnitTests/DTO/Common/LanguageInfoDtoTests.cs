#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.DTO.Common;

/// <summary>
/// Contains unit tests for the <see cref="LanguageInfoDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class LanguageInfoDtoTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void RoundTrip_WhenSerializingFullyPopulatedLanguageInfo_ShouldPreserveValues()
    {
        // Arrange
        LanguageInfoDto expected = new("en", "English", "English");

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        LanguageInfoDto? actual = JsonSerializer.Deserialize<LanguageInfoDto>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Equality_WhenTwoInstancesHaveSameValues_ShouldBeEqual()
    {
        // Arrange
        LanguageInfoDto first = new("fr", "French", "Français");
        LanguageInfoDto second = new("fr", "French", "Français");

        // Act
        bool areEqual = first == second;

        // Assert
        Assert.True(areEqual);
    }
}
