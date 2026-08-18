#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.DTO.Common;

/// <summary>
/// Contains unit tests for the <see cref="ReleaseInfoDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReleaseInfoDtoTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void RoundTrip_WhenSerializingFullyPopulatedReleaseInfo_ShouldPreserveValues()
    {
        // Arrange
        DateOnly originalReleaseDate = new(2005, 6, 15);
        DateOnly reReleaseDate = new(2015, 9, 1);
        ReleaseInfoDto expected = new(
            originalReleaseDate,
            2005,
            reReleaseDate,
            2015,
            "US",
            "Revised Edition"
        );

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        ReleaseInfoDto? actual = JsonSerializer.Deserialize<ReleaseInfoDto>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void RoundTrip_WhenSerializingEmptyReleaseInfo_ShouldPreserveNullValues()
    {
        // Arrange
        ReleaseInfoDto expected = new(null, null, null, null, null, null);

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        ReleaseInfoDto? actual = JsonSerializer.Deserialize<ReleaseInfoDto>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Equality_WhenTwoInstancesHaveSameValues_ShouldBeEqual()
    {
        // Arrange
        ReleaseInfoDto first = new(new DateOnly(2005, 6, 15), 2005, null, null, "US", null);
        ReleaseInfoDto second = new(new DateOnly(2005, 6, 15), 2005, null, null, "US", null);

        // Act
        bool areEqual = first == second;

        // Assert
        Assert.True(areEqual);
    }
}
