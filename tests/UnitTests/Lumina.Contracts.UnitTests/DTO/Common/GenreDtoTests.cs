#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.DTO.Common;

/// <summary>
/// Contains unit tests for the <see cref="GenreDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GenreDtoTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void RoundTrip_WhenSerializingGenre_ShouldPreserveName()
    {
        // Arrange
        GenreDto expected = new("Science Fiction");

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        GenreDto? actual = JsonSerializer.Deserialize<GenreDto>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Equality_WhenTwoInstancesHaveSameValues_ShouldBeEqual()
    {
        // Arrange
        GenreDto first = new("Fantasy");
        GenreDto second = new("Fantasy");

        // Act
        bool areEqual = first == second;

        // Assert
        Assert.True(areEqual);
    }
}
