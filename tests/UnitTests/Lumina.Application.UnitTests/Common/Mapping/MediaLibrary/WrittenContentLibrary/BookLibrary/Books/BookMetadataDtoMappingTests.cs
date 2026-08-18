#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
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
    private readonly BookMetadataDtoFixture _bookMetadataDtoFixture = new();

    [Fact]
    public void ApplyMetadata_WhenCalledWithValidMetadata_ShouldApplyItAndMarkTheBookAsEnriched()
    {
        // Arrange
        Book book = _bookFixture.Create();
        BookMetadataDto metadata = _bookMetadataDtoFixture.Create(
            title: "The Fellowship of the Ring",
            description: "The first part of J.R.R. Tolkien's epic adventure.",
            goodreadsId: "3",
            format: BookFormat.Paperback,
            publisher: "Houghton Mifflin",
            pageCount: 398);

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
        BookMetadataDto metadata = _bookMetadataDtoFixture.Create(
            title: "The Fellowship of the Ring",
            goodreadsId: "3") with
        {
            Genres = [new GenreDto("")]
        };

        // Act
        Result<Success> result = book.ApplyMetadata(metadata, "Goodreads", DateTime.UtcNow);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(MetadataStatus.Pending, book.MetadataStatus);
    }
}
