#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Contracts.UnitTests.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;

/// <summary>
/// Contains unit tests for the <see cref="BookMetadataLookupDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookMetadataLookupDtoTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    [Fact]
    public void RoundTrip_WhenSerializingLookupWithAllValues_ShouldPreserveValues()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        BookMetadataLookupDto expected = new(
            libraryId,
            @"C:\Media\Books\dune.epub",
            "978-0-306-40615-7",
            "OL12345W",
            "Dune",
            "Frank Herbert",
            "en"
        );

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        BookMetadataLookupDto? actual = JsonSerializer.Deserialize<BookMetadataLookupDto>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Constructor_WhenOmittingOptionalParameters_ShouldUseNullDefaults()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();

        // Act
        BookMetadataLookupDto sut = new(libraryId, @"/media/books/dune.epub");

        // Assert
        Assert.Equal(libraryId, sut.LibraryId);
        Assert.Equal(@"/media/books/dune.epub", sut.Path);
        Assert.Null(sut.Isbn);
        Assert.Null(sut.OpenLibraryId);
        Assert.Null(sut.Title);
        Assert.Null(sut.Author);
        Assert.Null(sut.LanguageCode);
    }
}
