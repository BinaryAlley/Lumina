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
    public void RoundTrip_WhenDeserializingJsonWithoutLibraryId_ShouldThrowJsonException()
    {
        // Arrange
        string json = """{ "searchTerm": "fantasy" }""";

        // Act & Assert
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<LibraryFilterDto>(json, _jsonOptions));
    }
}
