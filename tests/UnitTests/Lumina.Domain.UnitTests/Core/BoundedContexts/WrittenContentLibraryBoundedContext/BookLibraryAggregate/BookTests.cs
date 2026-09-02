#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.MediaContributorBoundedContext.MediaContributorAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.MediaContributorBoundedContext.MediaContributorAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate;

/// <summary>
/// Contains unit tests for the <see cref="Book"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookTests
{
    private readonly BookFixture _bookFixture = new();
    private readonly MediaContributorIdFixture _mediaContributorIdFixture = new();

    [Fact]
    public void Create_WhenCalledWithValidData_ShouldCreateBook()
    {
        // Act
        Book book = _bookFixture.Create();

        // Assert
        Assert.NotNull(book);
        Assert.NotNull(book.Metadata);
        Assert.NotNull(book.Path);
        Assert.NotNull(book.LibraryId);
    }

    [Fact]
    public void UpdateContributors_WhenCalled_ShouldReplaceTheContributors()
    {
        // Arrange
        Book book = _bookFixture.Create();
        MediaContributorId firstContributorId = _mediaContributorIdFixture.Create();
        MediaContributorId secondContributorId = _mediaContributorIdFixture.Create();

        // Act
        book.UpdateContributors([firstContributorId, secondContributorId]);

        // Assert
        Assert.Equal(2, book.Contributors.Count);
        Assert.Contains(book.Contributors, contributorId => contributorId == firstContributorId);
        Assert.Contains(book.Contributors, contributorId => contributorId == secondContributorId);
    }

    [Fact]
    public void UpdateContributors_WhenCalledAgain_ShouldReplaceTheContributors()
    {
        // Arrange
        Book book = _bookFixture.Create();
        book.UpdateContributors([_mediaContributorIdFixture.Create()]);
        MediaContributorId replacementContributorId = _mediaContributorIdFixture.Create();

        // Act
        book.UpdateContributors([replacementContributorId]);

        // Assert
        // the previous contributors are replaced, not appended
        MediaContributorId contributorId = Assert.Single(book.Contributors);
        Assert.Equal(replacementContributorId, contributorId);
    }

    [Fact]
    public void Create_WhenCalledWithPreExistingIdAndTimestamps_ShouldPreserveIdentityAndTimestamps()
    {
        // Arrange
        Book sourceBook = _bookFixture.Create();
        DateTime createdOnUtc = DateTime.UtcNow.AddDays(-1);
        DateTime updatedOnUtc = DateTime.UtcNow;

        // Act
        Result<Book> result = Book.Create(
            sourceBook.Id,
            sourceBook.LibraryId,
            sourceBook.Path,
            sourceBook.Metadata,
            sourceBook.Format,
            sourceBook.Edition,
            sourceBook.VolumeNumber,
            sourceBook.Series,
            sourceBook.ASIN,
            sourceBook.GoodreadsId,
            sourceBook.LCCN,
            sourceBook.OCLCNumber,
            sourceBook.OpenLibraryId,
            sourceBook.LibraryThingId,
            sourceBook.GoogleBooksId,
            sourceBook.BarnesAndNobleId,
            sourceBook.AppleBooksId,
            createdOnUtc,
            Optional<DateTime>.Some(updatedOnUtc),
            [.. sourceBook.ISBNs],
            [.. sourceBook.Contributors],
            [.. sourceBook.Ratings]);

        // Assert
        Book book = result.Value;
        Assert.Equal(sourceBook.Id, book.Id);
        Assert.Equal(sourceBook.LibraryId, book.LibraryId);
        Assert.Equal(sourceBook.Path, book.Path);
        Assert.Equal(sourceBook.Metadata, book.Metadata);
        Assert.Equal(createdOnUtc, book.CreatedOnUtc);
        Assert.Equal(updatedOnUtc, book.UpdatedOnUtc);
        Assert.Equal(sourceBook.ISBNs, book.ISBNs);
        Assert.Equal(sourceBook.Ratings, book.Ratings);
    }

    [Fact]
    public void ApplyEnrichedMetadata_WhenCalled_ShouldReplaceMetadataAndRelatedFields()
    {
        // Arrange
        Book book = _bookFixture.Create();
        Book enrichedSource = _bookFixture.Create();

        // Act
        book.ApplyEnrichedMetadata(
            enrichedSource.Metadata,
            enrichedSource.Format,
            enrichedSource.Edition,
            enrichedSource.VolumeNumber,
            enrichedSource.Series,
            enrichedSource.ASIN,
            enrichedSource.GoodreadsId,
            enrichedSource.LCCN,
            enrichedSource.OCLCNumber,
            enrichedSource.OpenLibraryId,
            enrichedSource.LibraryThingId,
            enrichedSource.GoogleBooksId,
            enrichedSource.BarnesAndNobleId,
            enrichedSource.AppleBooksId,
            [.. enrichedSource.ISBNs],
            [.. enrichedSource.Ratings]);

        // Assert
        // the metadata and related fields are replaced with the enriched values
        Assert.Equal(enrichedSource.Metadata, book.Metadata);
        Assert.Equal(enrichedSource.Format, book.Format);
        Assert.Equal(enrichedSource.Edition, book.Edition);
        Assert.Equal(enrichedSource.VolumeNumber, book.VolumeNumber);
        Assert.Equal(enrichedSource.Series, book.Series);
        Assert.Equal(enrichedSource.ASIN, book.ASIN);
        Assert.Equal(enrichedSource.GoodreadsId, book.GoodreadsId);
        Assert.Equal(enrichedSource.LCCN, book.LCCN);
        Assert.Equal(enrichedSource.OCLCNumber, book.OCLCNumber);
        Assert.Equal(enrichedSource.OpenLibraryId, book.OpenLibraryId);
        Assert.Equal(enrichedSource.LibraryThingId, book.LibraryThingId);
        Assert.Equal(enrichedSource.GoogleBooksId, book.GoogleBooksId);
        Assert.Equal(enrichedSource.BarnesAndNobleId, book.BarnesAndNobleId);
        Assert.Equal(enrichedSource.AppleBooksId, book.AppleBooksId);
        Assert.Equal(enrichedSource.ISBNs, book.ISBNs);
        Assert.Equal(enrichedSource.Ratings, book.Ratings);
    }

    [Fact]
    public void ApplyEnrichedMetadata_WhenCalledAgain_ShouldReplaceCollectionsWithoutAppending()
    {
        // Arrange
        Book book = _bookFixture.Create();
        Book firstEnrichmentSource = _bookFixture.Create();
        Book secondEnrichmentSource = _bookFixture.Create();
        book.ApplyEnrichedMetadata(
            firstEnrichmentSource.Metadata,
            firstEnrichmentSource.Format,
            firstEnrichmentSource.Edition,
            firstEnrichmentSource.VolumeNumber,
            firstEnrichmentSource.Series,
            firstEnrichmentSource.ASIN,
            firstEnrichmentSource.GoodreadsId,
            firstEnrichmentSource.LCCN,
            firstEnrichmentSource.OCLCNumber,
            firstEnrichmentSource.OpenLibraryId,
            firstEnrichmentSource.LibraryThingId,
            firstEnrichmentSource.GoogleBooksId,
            firstEnrichmentSource.BarnesAndNobleId,
            firstEnrichmentSource.AppleBooksId,
            [.. firstEnrichmentSource.ISBNs],
            [.. firstEnrichmentSource.Ratings]);

        // Act
        book.ApplyEnrichedMetadata(
            secondEnrichmentSource.Metadata,
            secondEnrichmentSource.Format,
            secondEnrichmentSource.Edition,
            secondEnrichmentSource.VolumeNumber,
            secondEnrichmentSource.Series,
            secondEnrichmentSource.ASIN,
            secondEnrichmentSource.GoodreadsId,
            secondEnrichmentSource.LCCN,
            secondEnrichmentSource.OCLCNumber,
            secondEnrichmentSource.OpenLibraryId,
            secondEnrichmentSource.LibraryThingId,
            secondEnrichmentSource.GoogleBooksId,
            secondEnrichmentSource.BarnesAndNobleId,
            secondEnrichmentSource.AppleBooksId,
            [.. secondEnrichmentSource.ISBNs],
            [.. secondEnrichmentSource.Ratings]);

        // Assert
        // the second enrichment replaces the first, it is not appended to it
        Assert.Equal(secondEnrichmentSource.ISBNs.Count, book.ISBNs.Count);
        Assert.Equal(secondEnrichmentSource.ISBNs, book.ISBNs);
        Assert.Equal(secondEnrichmentSource.Ratings, book.Ratings);
    }

    [Fact]
    public void Create_WhenCalled_ShouldNotExposeEnrichmentTracking()
    {
        // Arrange
        Book book = _bookFixture.Create();
        Type bookType = book.GetType();

        // Act
        bool hasMetadataStatus = bookType.GetProperty("MetadataStatus") is not null;
        bool hasLastMetadataUpdateUtc = bookType.GetProperty("LastMetadataUpdateUtc") is not null;
        bool hasMetadataProvider = bookType.GetProperty("MetadataProvider") is not null;
        bool hasCoverImagePath = bookType.GetProperty("CoverImagePath") is not null;

        // Assert
        // the enrichment state is a persistence concern, tracked on the repository entity by the enrichment jobs
        Assert.False(hasMetadataStatus);
        Assert.False(hasLastMetadataUpdateUtc);
        Assert.False(hasMetadataProvider);
        Assert.False(hasCoverImagePath);
    }
}
