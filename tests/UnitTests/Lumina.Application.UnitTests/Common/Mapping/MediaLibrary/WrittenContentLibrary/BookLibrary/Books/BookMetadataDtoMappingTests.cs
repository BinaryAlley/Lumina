#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Contains unit tests for the <see cref="BookMetadataDtoMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookMetadataDtoMappingTests
{
    private readonly BookFixture _bookFixture = new();

    [Fact]
    public void ApplyMetadata_WhenCalledWithValidMetadata_ShouldApplyItAndMarkTheBookAsEnriched()
    {
        // Arrange
        Book book = _bookFixture.Create();
        BookMetadataDto metadata = CreateBookMetadata("The Fellowship of the Ring", "3");

        // Act
        Result<Success> result = book.ApplyMetadata(metadata, "Goodreads", DateTime.UtcNow);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal("The Fellowship of the Ring", book.Metadata.Title);
        Assert.Equal("3", book.GoodreadsId.Value);
        Assert.Equal("The first part of J.R.R. Tolkien's epic adventure.", book.Metadata.Description.Value);
        Assert.Equal(BookFormat.Paperback, book.Format.Value);
        Assert.Equal("Houghton Mifflin", book.Metadata.Publisher.Value);
        Assert.Equal(MetadataStatus.Enriched, book.MetadataStatus);
        Assert.Equal("Goodreads", book.MetadataProvider.Value);
        Assert.True(book.LastMetadataUpdateUtc.HasValue);
    }

    [Fact]
    public void ApplyMetadata_WhenCalledWithInvalidGenres_ShouldReturnError()
    {
        // Arrange
        Book book = _bookFixture.Create();
        BookMetadataDto metadata = CreateBookMetadata("The Fellowship of the Ring", "3") with
        {
            Genres = [new GenreDto("")]
        };

        // Act
        Result<Success> result = book.ApplyMetadata(metadata, "Goodreads", DateTime.UtcNow);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(MetadataStatus.Pending, book.MetadataStatus);
    }

    private static BookMetadataDto CreateBookMetadata(string title, string goodreadsId)
    {
        return new BookMetadataDto(
            Title: title,
            OriginalTitle: title,
            Description: "The first part of J.R.R. Tolkien's epic adventure.",
            ReleaseInfo: new ReleaseInfoDto(
                OriginalReleaseDate: new DateOnly(1954, 7, 29),
                OriginalReleaseYear: 1954,
                ReReleaseDate: null,
                ReReleaseYear: null,
                ReleaseCountry: "uk",
                ReleaseVersion: null
            ),
            Genres: [new GenreDto("fantasy")],
            Tags: [new TagDto("epic fantasy")],
            Language: new LanguageInfoDto("en", "English", "English"),
            OriginalLanguage: null,
            Publisher: "Houghton Mifflin",
            PageCount: 398,
            Format: BookFormat.Paperback,
            Edition: null,
            VolumeNumber: 1,
            Series: null,
            ASIN: null,
            GoodreadsId: goodreadsId,
            LCCN: null,
            OCLCNumber: null,
            OpenLibraryId: null,
            LibraryThingId: null,
            GoogleBooksId: null,
            BarnesAndNobleId: null,
            AppleBooksId: null,
            Isbns: null,
            Contributors: null,
            Ratings: null,
            CoverImageUrl: null
        );
    }
}
