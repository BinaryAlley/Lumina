#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.Mapping.Common.Metadata;
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Application.UnitTests.Core.MediaLibrary.WrittenContentLibrary.BooksLibrary.Books.Commands.AddBook.Fixtures;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Contains unit tests for the <see cref="BookEntityMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookEntityMappingTests
{
    private readonly BookEntityFixture _bookEntityFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="BookEntityMappingTests"/> class.
    /// </summary>
    public BookEntityMappingTests()
    {
        _bookEntityFixture = new BookEntityFixture();
    }

    [Fact]
    public void ToDomainEntity_WhenMappingValidBookEntity_ShouldMapCorrectly()
    {
        // Arrange
        BookEntity bookEntity = _bookEntityFixture.CreateBook();

        // Act
        ErrorOr<Book> result = bookEntity.ToDomainEntity();

        // Assert
        Assert.False(result.IsError);
        Assert.NotNull(result.Value);
        Assert.Equal(bookEntity.Id, result.Value.Id.Value);
        Assert.Equal(bookEntity.Title, result.Value.Metadata.Title);

        if (bookEntity.OriginalTitle is not null)
        {
            Assert.True(result.Value.Metadata.OriginalTitle.HasValue);
            Assert.Equal(bookEntity.OriginalTitle, result.Value.Metadata.OriginalTitle.Value);
        }
        else
            Assert.False(result.Value.Metadata.OriginalTitle.HasValue);

        if (bookEntity.Description is not null)
        {
            Assert.True(result.Value.Metadata.Description.HasValue);
            Assert.Equal(bookEntity.Description, result.Value.Metadata.Description.Value);
        }
        else
            Assert.False(result.Value.Metadata.Description.HasValue);

        if (bookEntity.OriginalReleaseDate.HasValue)
        {
            Assert.True(result.Value.Metadata.ReleaseInfo.OriginalReleaseDate.HasValue);
            Assert.Equal(bookEntity.OriginalReleaseDate.Value, result.Value.Metadata.ReleaseInfo.OriginalReleaseDate.Value);
        }
        else
            Assert.False(result.Value.Metadata.ReleaseInfo.OriginalReleaseDate.HasValue);

        if (bookEntity.OriginalReleaseYear.HasValue)
        {
            Assert.True(result.Value.Metadata.ReleaseInfo.OriginalReleaseYear.HasValue);
            Assert.Equal(bookEntity.OriginalReleaseYear.Value, result.Value.Metadata.ReleaseInfo.OriginalReleaseYear.Value);
        }
        else
            Assert.False(result.Value.Metadata.ReleaseInfo.OriginalReleaseYear.HasValue);

        if (bookEntity.ReReleaseDate.HasValue)
        {
            Assert.True(result.Value.Metadata.ReleaseInfo.ReReleaseDate.HasValue);
            Assert.Equal(bookEntity.ReReleaseDate.Value, result.Value.Metadata.ReleaseInfo.ReReleaseDate.Value);
        }
        else
            Assert.False(result.Value.Metadata.ReleaseInfo.ReReleaseDate.HasValue);

        if (bookEntity.ReReleaseYear.HasValue)
        {
            Assert.True(result.Value.Metadata.ReleaseInfo.ReReleaseYear.HasValue);
            Assert.Equal(bookEntity.ReReleaseYear.Value, result.Value.Metadata.ReleaseInfo.ReReleaseYear.Value);
        }
        else
            Assert.False(result.Value.Metadata.ReleaseInfo.ReReleaseYear.HasValue);

        if (bookEntity.ReleaseCountry is not null)
        {
            Assert.True(result.Value.Metadata.ReleaseInfo.ReleaseCountry.HasValue);
            Assert.Equal(bookEntity.ReleaseCountry, result.Value.Metadata.ReleaseInfo.ReleaseCountry.Value);
        }
        else
            Assert.False(result.Value.Metadata.ReleaseInfo.ReleaseCountry.HasValue);

        if (bookEntity.ReleaseVersion is not null)
        {
            Assert.True(result.Value.Metadata.ReleaseInfo.ReleaseVersion.HasValue);
            Assert.Equal(bookEntity.ReleaseVersion, result.Value.Metadata.ReleaseInfo.ReleaseVersion.Value);
        }
        else
            Assert.False(result.Value.Metadata.ReleaseInfo.ReleaseVersion.HasValue);

        if (bookEntity.LanguageCode is not null)
        {
            Assert.True(result.Value.Metadata.Language.HasValue);
            Assert.Equal(bookEntity.LanguageCode, result.Value.Metadata.Language.Value.LanguageCode);
            Assert.Equal(bookEntity.LanguageName, result.Value.Metadata.Language.Value.LanguageName);

            if (bookEntity.LanguageNativeName is not null)
            {
                Assert.True(result.Value.Metadata.Language.Value.NativeName.HasValue);
                Assert.Equal(bookEntity.LanguageNativeName, result.Value.Metadata.Language.Value.NativeName.Value);
            }
            else
                Assert.False(result.Value.Metadata.Language.Value.NativeName.HasValue);
        }
        else
            Assert.False(result.Value.Metadata.Language.HasValue);

        if (bookEntity.OriginalLanguageCode is not null)
        {
            Assert.True(result.Value.Metadata.OriginalLanguage.HasValue);
            Assert.Equal(bookEntity.OriginalLanguageCode, result.Value.Metadata.OriginalLanguage.Value.LanguageCode);
            Assert.Equal(bookEntity.OriginalLanguageName, result.Value.Metadata.OriginalLanguage.Value.LanguageName);

            if (bookEntity.OriginalLanguageNativeName is not null)
            {
                Assert.True(result.Value.Metadata.OriginalLanguage.Value.NativeName.HasValue);
                Assert.Equal(bookEntity.OriginalLanguageNativeName, result.Value.Metadata.OriginalLanguage.Value.NativeName.Value);
            }
            else
                Assert.False(result.Value.Metadata.OriginalLanguage.Value.NativeName.HasValue);
        }
        else
            Assert.False(result.Value.Metadata.OriginalLanguage.HasValue);

        if (bookEntity.Publisher is not null)
        {
            Assert.True(result.Value.Metadata.Publisher.HasValue);
            Assert.Equal(bookEntity.Publisher, result.Value.Metadata.Publisher.Value);
        }
        else
            Assert.False(result.Value.Metadata.Publisher.HasValue);

        if (bookEntity.PageCount.HasValue)
        {
            Assert.True(result.Value.Metadata.PageCount.HasValue);
            Assert.Equal(bookEntity.PageCount.Value, result.Value.Metadata.PageCount.Value);
        }
        else
            Assert.False(result.Value.Metadata.PageCount.HasValue);

        Assert.Equal(bookEntity.Format, result.Value.Format.Value);

        if (bookEntity.Edition is not null)
        {
            Assert.True(result.Value.Edition.HasValue);
            Assert.Equal(bookEntity.Edition, result.Value.Edition.Value);
        }
        else
            Assert.False(result.Value.Edition.HasValue);

        if (bookEntity.VolumeNumber.HasValue)
        {
            Assert.True(result.Value.VolumeNumber.HasValue);
            Assert.Equal(bookEntity.VolumeNumber.Value, result.Value.VolumeNumber.Value);
        }
        else
            Assert.False(result.Value.VolumeNumber.HasValue);

        if (bookEntity.ASIN is not null)
        {
            Assert.True(result.Value.ASIN.HasValue);
            Assert.Equal(bookEntity.ASIN, result.Value.ASIN.Value);
        }
        else
            Assert.False(result.Value.ASIN.HasValue);

        if (bookEntity.GoodreadsId is not null)
        {
            Assert.True(result.Value.GoodreadsId.HasValue);
            Assert.Equal(bookEntity.GoodreadsId, result.Value.GoodreadsId.Value);
        }
        else
            Assert.False(result.Value.GoodreadsId.HasValue);

        if (bookEntity.LCCN is not null)
        {
            Assert.True(result.Value.LCCN.HasValue);
            Assert.Equal(bookEntity.LCCN, result.Value.LCCN.Value);
        }
        else
            Assert.False(result.Value.LCCN.HasValue);

        if (bookEntity.OCLCNumber is not null)
        {
            Assert.True(result.Value.OCLCNumber.HasValue);
            Assert.Equal(bookEntity.OCLCNumber, result.Value.OCLCNumber.Value);
        }
        else
            Assert.False(result.Value.OCLCNumber.HasValue);

        if (bookEntity.OpenLibraryId is not null)
        {
            Assert.True(result.Value.OpenLibraryId.HasValue);
            Assert.Equal(bookEntity.OpenLibraryId, result.Value.OpenLibraryId.Value);
        }
        else
            Assert.False(result.Value.OpenLibraryId.HasValue);

        if (bookEntity.LibraryThingId is not null)
        {
            Assert.True(result.Value.LibraryThingId.HasValue);
            Assert.Equal(bookEntity.LibraryThingId, result.Value.LibraryThingId.Value);
        }
        else
            Assert.False(result.Value.LibraryThingId.HasValue);

        if (bookEntity.GoogleBooksId is not null)
        {
            Assert.True(result.Value.GoogleBooksId.HasValue);
            Assert.Equal(bookEntity.GoogleBooksId, result.Value.GoogleBooksId.Value);
        }
        else
            Assert.False(result.Value.GoogleBooksId.HasValue);

        if (bookEntity.BarnesAndNobleId is not null)
        {
            Assert.True(result.Value.BarnesAndNobleId.HasValue);
            Assert.Equal(bookEntity.BarnesAndNobleId, result.Value.BarnesAndNobleId.Value);
        }
        else
            Assert.False(result.Value.BarnesAndNobleId.HasValue);

        if (bookEntity.AppleBooksId is not null)
        {
            Assert.True(result.Value.AppleBooksId.HasValue);
            Assert.Equal(bookEntity.AppleBooksId, result.Value.AppleBooksId.Value);
        }
        else
            Assert.False(result.Value.AppleBooksId.HasValue);

        Assert.Equal(bookEntity.CreatedOnUtc, result.Value.CreatedOnUtc);

        if (bookEntity.UpdatedOnUtc.HasValue)
        {
            Assert.True(result.Value.UpdatedOnUtc.HasValue);
            Assert.Equal(bookEntity.UpdatedOnUtc.Value, result.Value.UpdatedOnUtc!.Value);
        }
        else
            Assert.False(result.Value.UpdatedOnUtc.HasValue);
    }

    [Fact]
    public void ToResponse_WhenMappingBookEntity_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        BookEntity bookEntity = _bookEntityFixture.CreateBook();

        // Act
        BookResponse result = bookEntity.ToResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(bookEntity.Id, result.Id);
        Assert.Equal(bookEntity.Title, result.Metadata.Title);
        Assert.Equal(bookEntity.OriginalTitle, result.Metadata.OriginalTitle);
        Assert.Equal(bookEntity.Description, result.Metadata.Description);
        Assert.Equal(bookEntity.OriginalReleaseDate, result.Metadata.ReleaseInfo!.OriginalReleaseDate);
        Assert.Equal(bookEntity.OriginalReleaseYear, result.Metadata.ReleaseInfo.OriginalReleaseYear);
        Assert.Equal(bookEntity.ReReleaseDate, result.Metadata.ReleaseInfo.ReReleaseDate);
        Assert.Equal(bookEntity.ReReleaseYear, result.Metadata.ReleaseInfo.ReReleaseYear);
        Assert.Equal(bookEntity.ReleaseCountry, result.Metadata.ReleaseInfo.ReleaseCountry);
        Assert.Equal(bookEntity.ReleaseVersion, result.Metadata.ReleaseInfo.ReleaseVersion);
        Assert.NotNull(result.Metadata.Language);
        Assert.Equal(bookEntity.LanguageCode, result.Metadata.Language!.LanguageCode);
        Assert.Equal(bookEntity.LanguageName, result.Metadata.Language.LanguageName);
        Assert.Equal(bookEntity.LanguageNativeName, result.Metadata.Language.NativeName);
        Assert.NotNull(result.Metadata.OriginalLanguage);
        Assert.Equal(bookEntity.OriginalLanguageCode, result.Metadata.OriginalLanguage!.LanguageCode);
        Assert.Equal(bookEntity.OriginalLanguageName, result.Metadata.OriginalLanguage.LanguageName);
        Assert.Equal(bookEntity.OriginalLanguageNativeName, result.Metadata.OriginalLanguage.NativeName);
        Assert.Equal(bookEntity.Publisher, result.Metadata.Publisher);
        Assert.Equal(bookEntity.PageCount, result.Metadata.PageCount);
        Assert.Equal(bookEntity.Format, result.Format);
        Assert.Equal(bookEntity.Edition, result.Edition);
        Assert.Equal(bookEntity.VolumeNumber, result.VolumeNumber);
        Assert.Equal(bookEntity.ASIN, result.ASIN);
        Assert.Equal(bookEntity.GoodreadsId, result.GoodreadsId);
        Assert.Equal(bookEntity.LCCN, result.LCCN);
        Assert.Equal(bookEntity.OCLCNumber, result.OCLCNumber);
        Assert.Equal(bookEntity.OpenLibraryId, result.OpenLibraryId);
        Assert.Equal(bookEntity.LibraryThingId, result.LibraryThingId);
        Assert.Equal(bookEntity.GoogleBooksId, result.GoogleBooksId);
        Assert.Equal(bookEntity.BarnesAndNobleId, result.BarnesAndNobleId);
        Assert.Equal(bookEntity.AppleBooksId, result.AppleBooksId);
        Assert.Equal(bookEntity.ISBNs.Select(i => new IsbnDto(i.Value, i.Format)), result.ISBNs);
        Assert.Equal(bookEntity.Ratings.Select(r => new BookRatingDto(
            r.Value,
            r.MaxValue,
            r.Source,
            r.VoteCount
        )), result.Ratings);
        Assert.Equal(bookEntity.CreatedOnUtc, result.CreatedOnUtc);
        Assert.Equal(bookEntity.UpdatedOnUtc, result.UpdatedOnUtc);
    }

    [Fact]
    public void ToDomainModels_WhenMappingMultipleBookEntities_ShouldMapAllCorrectly()
    {
        // Arrange
        List<BookEntity> bookEntities =
        [
            _bookEntityFixture.CreateBook(),
            _bookEntityFixture.CreateBook()
        ];

        // Act
        IEnumerable<ErrorOr<Book>> results = bookEntities.ToDomainEntities();

        // Assert
        Assert.NotNull(results);
        Assert.Equal(bookEntities.Count, results.Count());
        Assert.All(results, result => Assert.False(result.IsError));
    }

    [Fact]
    public void ToResponses_WhenMappingMultipleBookEntities_ShouldMapAllCorrectly()
    {
        // Arrange
        List<BookEntity> bookEntities =
        [
            _bookEntityFixture.CreateBook(),
            _bookEntityFixture.CreateBook()
        ];

        // Act
        IEnumerable<BookResponse> results = bookEntities.ToResponses();

        // Assert
        Assert.NotNull(results);
        Assert.Equal(bookEntities.Count, results.Count());
    }
}
