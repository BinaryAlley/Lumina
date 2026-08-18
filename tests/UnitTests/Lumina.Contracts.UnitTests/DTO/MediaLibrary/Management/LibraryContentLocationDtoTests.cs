#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.MediaLibrary.Management;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.DTO.MediaLibrary.Management;

/// <summary>
/// Contains unit tests for the <see cref="LibraryContentLocationDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryContentLocationDtoTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void RoundTrip_WhenSerializingContentLocation_ShouldPreservePath()
    {
        // Arrange
        LibraryContentLocationDto expected = new(@"C:\Media\Books");

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        LibraryContentLocationDto? actual = JsonSerializer.Deserialize<LibraryContentLocationDto>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Equality_WhenTwoInstancesHaveSameValues_ShouldBeEqual()
    {
        // Arrange
        LibraryContentLocationDto first = new(@"/media/books");
        LibraryContentLocationDto second = new(@"/media/books");

        // Act
        bool areEqual = first == second;

        // Assert
        Assert.True(areEqual);
    }
}
