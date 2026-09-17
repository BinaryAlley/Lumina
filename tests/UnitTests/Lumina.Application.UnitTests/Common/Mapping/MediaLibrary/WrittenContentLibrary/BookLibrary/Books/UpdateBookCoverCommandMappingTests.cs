#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.UpdateBookCover;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Fixtures.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.UpdateBookCover;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Contains unit tests for the <see cref="UpdateBookCoverCommandMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateBookCoverCommandMappingTests
{
    private readonly UpdateBookCoverCommandFixture _commandFixture = new();
    private readonly BookArtworkEntityFixture _bookArtworkEntityFixture = new();

    [Fact]
    public void ToRepositoryEntity_WhenBookHasNoCover_ShouldReturnANewEnrichedCoverArtwork()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid bookId = Guid.NewGuid();
        UpdateBookCoverCommand command = _commandFixture.Create(bookId: bookId.ToString());
        string storedCoverPath = "/media/books/cover.jpg";

        // Act
        BookArtworkEntity cover = command.ToRepositoryEntity(null, bookId, storedCoverPath, userId);

        // Assert
        Assert.Equal(bookId, cover.BookId);
        Assert.Equal(ArtworkType.Cover, cover.ArtworkType);
        Assert.Equal(0, cover.Ordinal);
        Assert.Equal(storedCoverPath, cover.FileName);
        Assert.Equal(ArtworkStatus.Enriched, cover.Status);
        Assert.Equal(userId, cover.CreatedBy);
        Assert.NotEqual(default, cover.CreatedOnUtc);
        Assert.Null(cover.UpdatedBy);
    }

    [Fact]
    public void ToRepositoryEntity_WhenBookHasAnExistingCover_ShouldReplaceAndReturnTheStoredCover()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid bookId = Guid.NewGuid();
        UpdateBookCoverCommand command = _commandFixture.Create(bookId: bookId.ToString());
        BookArtworkEntity existingCover = _bookArtworkEntityFixture.Create(bookId: bookId, artworkType: ArtworkType.Cover, fileName: "/media/books/old-cover.jpg", status: ArtworkStatus.Pending);
        string storedCoverPath = "/media/books/cover.jpg";

        // Act
        BookArtworkEntity cover = command.ToRepositoryEntity(existingCover, bookId, storedCoverPath, userId);

        // Assert
        Assert.Same(existingCover, cover);
        Assert.Equal(storedCoverPath, cover.FileName);
        Assert.Equal(ArtworkStatus.Enriched, cover.Status);
        Assert.Equal(userId, cover.UpdatedBy);
        Assert.NotNull(cover.UpdatedOnUtc);
        Assert.NotNull(cover.LastUpdateUtc);
    }
}
