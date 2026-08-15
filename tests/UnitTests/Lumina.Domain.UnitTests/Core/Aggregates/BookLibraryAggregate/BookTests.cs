#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.Aggregates.BookLibraryAggregate;

/// <summary>
/// Contains unit tests for the <see cref="Book"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookTests
{
    private readonly BookFixture _bookFixture = new();

    [Fact]
    public void Create_WhenCalledWithValidData_ShouldCreateBookWithPendingMetadataStatus()
    {
        // Act
        Book book = _bookFixture.Create();

        // Assert
        Assert.NotNull(book);
        Assert.Equal(MetadataStatus.Pending, book.MetadataStatus);
        Assert.False(book.LastMetadataUpdateUtc.HasValue);
        Assert.False(book.MetadataProvider.HasValue);
    }

    [Fact]
    public void MarkMetadataAsEnriched_WhenCalled_ShouldUpdateStatusAndProvider()
    {
        // Arrange
        Book book = _bookFixture.Create();
        DateTime lastUpdateUtc = DateTime.UtcNow;

        // Act
        book.MarkMetadataAsEnriched("Goodreads", lastUpdateUtc);

        // Assert
        Assert.Equal(MetadataStatus.Enriched, book.MetadataStatus);
        Assert.Equal("Goodreads", book.MetadataProvider.Value);
        Assert.Equal(lastUpdateUtc, book.LastMetadataUpdateUtc.Value);
    }

    [Fact]
    public void MarkMetadataAsFailed_WhenCalled_ShouldUpdateStatus()
    {
        // Arrange
        Book book = _bookFixture.Create();

        // Act
        book.MarkMetadataAsFailed();

        // Assert
        Assert.Equal(MetadataStatus.Failed, book.MetadataStatus);
    }
}
