#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.DTO.MediaLibrary.WrittenContentLibrary;

/// <summary>
/// Contains unit tests for the <see cref="WrittenContentMetadataDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class WrittenContentMetadataDtoTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void RoundTrip_WhenSerializingFullyPopulatedMetadata_ShouldPreserveValues()
    {
        // Arrange
        WrittenContentMetadataDto expected = new(
            "Dune",
            "Dune",
            "A science fiction novel.",
            new ReleaseInfoDto(new DateOnly(1965, 8, 1), 1965, null, null, "US", null),
            [new GenreDto("Science Fiction")],
            [new TagDto("classic")],
            new LanguageInfoDto("en", "English", "English"),
            null,
            "Chilton Books",
            412
        );

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        WrittenContentMetadataDto? actual = JsonSerializer.Deserialize<WrittenContentMetadataDto>(json, _jsonOptions);
        // Assert
        Assert.NotNull(actual);
        Assert.Equivalent(expected, actual);
    }

    [Fact]
    public void RoundTrip_WhenSerializingMetadataWithoutOptionalValues_ShouldPreserveNullValues()
    {
        // Arrange
        WrittenContentMetadataDto expected = new(
            "Title",
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null
        );

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        WrittenContentMetadataDto? actual = JsonSerializer.Deserialize<WrittenContentMetadataDto>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }
}
