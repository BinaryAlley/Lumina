#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.Mapping.Common.Metadata;
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Common;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Contains unit tests for the <see cref="BookMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookMappingTests
{
    private readonly BookFixture _bookFixture = new();
    private readonly BookEntityFixture _bookEntityFixture = new();

    [Fact]
    public void ToRepositoryEntity_WhenMappingCompleteBook_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        Book book = _bookFixture.Create();

        // Act
        BookEntity result = book.ToRepositoryEntity();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(book.Id.Value, result.Id);
        Assert.Equal(book.Metadata.Title, result.Title);
        Assert.Equal(book.Metadata.OriginalTitle.HasValue ? book.Metadata.OriginalTitle.Value : null, result.OriginalTitle);
        Assert.Equal(book.Metadata.Description.HasValue ? book.Metadata.Description.Value : null, result.Description);
        Assert.Equal(book.Metadata.ReleaseInfo.OriginalReleaseDate.HasValue ? book.Metadata.ReleaseInfo.OriginalReleaseDate.Value : null, result.OriginalReleaseDate);
        Assert.Equal(book.Metadata.ReleaseInfo.OriginalReleaseYear.HasValue ? book.Metadata.ReleaseInfo.OriginalReleaseYear.Value : null, result.OriginalReleaseYear);
        Assert.Equal(book.Metadata.ReleaseInfo.ReReleaseDate.HasValue ? book.Metadata.ReleaseInfo.ReReleaseDate.Value : null, result.ReReleaseDate);
        Assert.Equal(book.Metadata.ReleaseInfo.ReReleaseYear.HasValue ? book.Metadata.ReleaseInfo.ReReleaseYear.Value : null, result.ReReleaseYear);
        Assert.Equal(book.Metadata.ReleaseInfo.ReleaseCountry.HasValue ? book.Metadata.ReleaseInfo.ReleaseCountry.Value : null, result.ReleaseCountry);
        Assert.Equal(book.Metadata.ReleaseInfo.ReleaseVersion.HasValue ? book.Metadata.ReleaseInfo.ReleaseVersion.Value : null, result.ReleaseVersion);
        Assert.Equal(book.Metadata.Language.HasValue ? book.Metadata.Language.Value.LanguageCode : null, result.LanguageCode);
        Assert.Equal(book.Metadata.Language.HasValue ? book.Metadata.Language.Value.LanguageName : null, result.LanguageName);
        Assert.Equal(book.Metadata.Language.HasValue ? book.Metadata.Language.Value.NativeName.Value : null, result.LanguageNativeName);
        Assert.Equal(book.Metadata.OriginalLanguage.HasValue ? book.Metadata.OriginalLanguage.Value.LanguageCode : null, result.OriginalLanguageCode);
        Assert.Equal(book.Metadata.OriginalLanguage.HasValue ? book.Metadata.OriginalLanguage.Value.LanguageName : null, result.OriginalLanguageName);
        Assert.Equal(book.Metadata.OriginalLanguage.HasValue ? book.Metadata.OriginalLanguage.Value.NativeName.Value : null, result.OriginalLanguageNativeName);
        Assert.Equal(book.Metadata.Tags.ToRepositoryEntities().OrderBy(tag => tag.Name), result.Tags.OrderBy(tag => tag.Name));
        Assert.Equal(book.Metadata.Genres.ToRepositoryEntities().OrderBy(genre => genre.Name), result.Genres.OrderBy(genre => genre.Name));
        Assert.Equal(book.Metadata.Publisher.HasValue ? book.Metadata.Publisher.Value : null, result.Publisher);
        Assert.Equal(book.Metadata.PageCount.HasValue ? book.Metadata.PageCount.Value : null, result.PageCount);
        Assert.Equal(book.Format.HasValue ? book.Format.Value : null, result.Format);
        Assert.Equal(book.Edition.HasValue ? book.Edition.Value : null, result.Edition);
        Assert.Equal(book.VolumeNumber.HasValue ? book.VolumeNumber.Value : null, result.VolumeNumber);
        Assert.Equal(book.ASIN.HasValue ? book.ASIN.Value : null, result.ASIN);
        Assert.Equal(book.GoodreadsId.HasValue ? book.GoodreadsId.Value : null, result.GoodreadsId);
        Assert.Equal(book.LCCN.HasValue ? book.LCCN.Value : null, result.LCCN);
        Assert.Equal(book.OCLCNumber.HasValue ? book.OCLCNumber.Value : null, result.OCLCNumber);
        Assert.Equal(book.OpenLibraryId.HasValue ? book.OpenLibraryId.Value : null, result.OpenLibraryId);
        Assert.Equal(book.LibraryThingId.HasValue ? book.LibraryThingId.Value : null, result.LibraryThingId);
        Assert.Equal(book.GoogleBooksId.HasValue ? book.GoogleBooksId.Value : null, result.GoogleBooksId);
        Assert.Equal(book.BarnesAndNobleId.HasValue ? book.BarnesAndNobleId.Value : null, result.BarnesAndNobleId);
        Assert.Equal(book.AppleBooksId.HasValue ? book.AppleBooksId.Value : null, result.AppleBooksId);
        Assert.Equal(book.ISBNs.ToRepositoryEntities(), result.ISBNs);
        Assert.Equal(book.Ratings.ToRepositoryEntities(), result.Ratings);
        Assert.Equal(book.CreatedOnUtc, result.CreatedOnUtc);
        Assert.Equal(book.UpdatedOnUtc.HasValue ? book.UpdatedOnUtc.Value : null, result.UpdatedOnUtc);
    }

    [Fact]
    public void ApplyMetadataToEntity_WhenApplyingCompleteBook_ShouldOverwriteTheMetadataFields()
    {
        // Arrange
        Book book = _bookFixture.Create();
        BookEntity entity = _bookEntityFixture.Create();
        BookEntity expectedEntity = book.ToRepositoryEntity();

        // Act
        entity.ApplyMetadataToEntity(book);

        // Assert
        Assert.Equal(book.Metadata.Title, entity.Title);
        Assert.Equal(expectedEntity.OriginalTitle, entity.OriginalTitle);
        Assert.Equal(expectedEntity.Description, entity.Description);
        Assert.Equal(expectedEntity.OriginalReleaseDate, entity.OriginalReleaseDate);
        Assert.Equal(expectedEntity.OriginalReleaseYear, entity.OriginalReleaseYear);
        Assert.Equal(expectedEntity.ReReleaseDate, entity.ReReleaseDate);
        Assert.Equal(expectedEntity.ReReleaseYear, entity.ReReleaseYear);
        Assert.Equal(expectedEntity.ReleaseCountry, entity.ReleaseCountry);
        Assert.Equal(expectedEntity.ReleaseVersion, entity.ReleaseVersion);
        Assert.Equal(expectedEntity.LanguageCode, entity.LanguageCode);
        Assert.Equal(expectedEntity.LanguageName, entity.LanguageName);
        Assert.Equal(expectedEntity.LanguageNativeName, entity.LanguageNativeName);
        Assert.Equal(expectedEntity.OriginalLanguageCode, entity.OriginalLanguageCode);
        Assert.Equal(expectedEntity.OriginalLanguageName, entity.OriginalLanguageName);
        Assert.Equal(expectedEntity.OriginalLanguageNativeName, entity.OriginalLanguageNativeName);
        Assert.Equal(expectedEntity.Publisher, entity.Publisher);
        Assert.Equal(expectedEntity.PageCount, entity.PageCount);
        Assert.Equal(expectedEntity.Format, entity.Format);
        Assert.Equal(expectedEntity.Edition, entity.Edition);
        Assert.Equal(expectedEntity.VolumeNumber, entity.VolumeNumber);
        Assert.Equal(expectedEntity.ASIN, entity.ASIN);
        Assert.Equal(expectedEntity.GoodreadsId, entity.GoodreadsId);
        Assert.Equal(expectedEntity.LCCN, entity.LCCN);
        Assert.Equal(expectedEntity.OCLCNumber, entity.OCLCNumber);
        Assert.Equal(expectedEntity.OpenLibraryId, entity.OpenLibraryId);
        Assert.Equal(expectedEntity.LibraryThingId, entity.LibraryThingId);
        Assert.Equal(expectedEntity.GoogleBooksId, entity.GoogleBooksId);
        Assert.Equal(expectedEntity.BarnesAndNobleId, entity.BarnesAndNobleId);
        Assert.Equal(expectedEntity.AppleBooksId, entity.AppleBooksId);
        Assert.NotNull(entity.UpdatedOnUtc);
        Assert.NotNull(entity.UpdatedBy);
    }

    [Fact]
    public void ApplyMetadataToEntity_WhenApplyingBook_ShouldPreserveIdentityAndCollectionColumns()
    {
        // Arrange
        Book book = _bookFixture.Create();
        BookEntity entity = _bookEntityFixture.Create();
        Guid entityId = entity.Id;
        Guid libraryId = entity.LibraryId;
        string path = entity.Path;
        int isbnsCount = entity.ISBNs.Count;
        int ratingsCount = entity.Ratings.Count;

        // Act
        entity.ApplyMetadataToEntity(book);

        // Assert
        // the identity and the collection columns are not part of the metadata application
        Assert.Equal(entityId, entity.Id);
        Assert.Equal(libraryId, entity.LibraryId);
        Assert.Equal(path, entity.Path);
        Assert.Equal(isbnsCount, entity.ISBNs.Count);
        Assert.Equal(ratingsCount, entity.Ratings.Count);
    }
}
