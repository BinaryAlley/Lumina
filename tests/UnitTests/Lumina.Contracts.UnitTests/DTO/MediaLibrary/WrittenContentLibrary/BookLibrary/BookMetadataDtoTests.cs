#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Contracts.UnitTests.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;

/// <summary>
/// Contains unit tests for the <see cref="BookMetadataDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookMetadataDtoTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    [Fact]
    public void RoundTrip_WhenSerializingFullyPopulatedMetadata_ShouldPreserveValues()
    {
        // Arrange
        BookMetadataDto expected = new(
            "Dune",
            "Dune",
            "A science fiction novel.",
            new ReleaseInfoDto(new DateOnly(1965, 8, 1), 1965, null, null, "US", null),
            [new GenreDto("Science Fiction")],
            [new TagDto("classic")],
            new LanguageInfoDto("en", "English", "English"),
            null,
            "Chilton Books",
            412,
            BookFormat.Hardcover,
            "First Edition",
            1,
            new BookSeriesDto("Dune Chronicles"),
            "B0006C7V9I",
            "12345",
            "68008446",
            "ocm01234567",
            "OL12345W",
            "lccn123",
            "abc123",
            "1234567890",
            "id12345",
            [new IsbnDto("978-0-306-40615-7", IsbnFormat.Isbn13)],
            [new MediaContributorDto(new MediaContributorNameDto("Frank Herbert", "Frank Herbert"), new MediaContributorRoleDto("Author", MediaContributorRoleCategory.Author))],
            [new BookRatingDto(4.5m, 5m, BookRatingSource.Goodreads, 1000)],
            "https://example.com/dune.jpg"
        );

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        BookMetadataDto? actual = JsonSerializer.Deserialize<BookMetadataDto>(json, _jsonOptions);
        // Assert
        Assert.NotNull(actual);
        Assert.Equivalent(expected, actual);
    }

    [Fact]
    public void Serialize_WhenSerializingMetadata_ShouldSerializeFormatAsCamelCaseString()
    {
        // Arrange
        BookMetadataDto sut = new(
            "Dune",
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            BookFormat.Paperback,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
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
        string json = JsonSerializer.Serialize(sut, _jsonOptions);

        // Assert
        Assert.Contains("\"Format\":\"paperback\"", json, StringComparison.Ordinal);
    }
}
