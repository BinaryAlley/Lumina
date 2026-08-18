#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common.Filtering;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.DTO.Common.Filtering;

/// <summary>
/// Contains unit tests for the <see cref="LibraryFilterDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryFilterDtoTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void LibraryFilterDto_WhenConstructed_ShouldRequireLibraryId()
    {
        // Act
        LibraryFilterDto sut = new() { LibraryId = Guid.NewGuid(), SearchTerm = "test" };

        // Assert
        Assert.NotEqual(Guid.Empty, sut.LibraryId);
    }

    [Fact]
    public void RoundTrip_WhenSerializingWithLibraryIdAndSearchTerm_ShouldPreserveValues()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        LibraryFilterDto expected = new() { LibraryId = libraryId, SearchTerm = "fantasy" };

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        LibraryFilterDto? actual = JsonSerializer.Deserialize<LibraryFilterDto>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected.LibraryId, actual.LibraryId);
        Assert.Equal(expected.SearchTerm, actual.SearchTerm);
    }

    [Fact]
    public void RoundTrip_WhenDeserializingJsonWithoutLibraryId_ShouldThrowJsonException()
    {
        // Arrange
        string json = """{ "searchTerm": "fantasy" }""";

        // Act & Assert
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<LibraryFilterDto>(json, _jsonOptions));
    }
}
