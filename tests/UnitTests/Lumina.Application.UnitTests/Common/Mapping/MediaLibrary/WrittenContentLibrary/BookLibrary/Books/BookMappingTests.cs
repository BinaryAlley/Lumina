#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.Common.Metadata;
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Common;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate;
using System.Diagnostics.CodeAnalysis;
using Lumina.Application.UnitTests.Core.MediaLibrary.WrittenContentLibrary.BooksLibrary.Books.Commands.AddBook.Fixtures;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Contains unit tests for the <see cref="BookMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookMappingTests
{
    private readonly BookFixture _bookFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="BookMappingTests"/> class.
    /// </summary>
    public BookMappingTests()
    {
        _bookFixture = new BookFixture();
    }

    [Fact]
    public void ToRepositoryEntity_WhenMappingCompleteBook_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        Book book = _bookFixture.CreateDomainBook();

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
        Assert.Equal(book.Metadata.Tags.ToRepositoryEntities(), result.Tags);
        Assert.Equal(book.Metadata.Genres.ToRepositoryEntities(), result.Genres);
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
}
