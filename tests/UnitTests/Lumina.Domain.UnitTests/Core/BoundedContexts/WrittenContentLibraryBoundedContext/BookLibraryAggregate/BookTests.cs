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
