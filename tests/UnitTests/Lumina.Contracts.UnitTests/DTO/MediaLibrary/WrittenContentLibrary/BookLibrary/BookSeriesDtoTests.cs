#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;

/// <summary>
/// Contains unit tests for the <see cref="BookSeriesDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookSeriesDtoTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void RoundTrip_WhenSerializingSeries_ShouldPreserveTitle()
    {
        // Arrange
        BookSeriesDto expected = new("The Wheel of Time");

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        BookSeriesDto? actual = JsonSerializer.Deserialize<BookSeriesDto>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Equality_WhenTwoInstancesHaveSameValues_ShouldBeEqual()
    {
        // Arrange
        BookSeriesDto first = new("Dune");
        BookSeriesDto second = new("Dune");

        // Act
        bool areEqual = first == second;

        // Assert
        Assert.True(areEqual);
    }
}
