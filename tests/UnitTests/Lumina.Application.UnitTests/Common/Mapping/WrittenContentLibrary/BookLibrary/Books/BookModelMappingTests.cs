#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FluentAssertions;
using Lumina.Application.Common.Mapping.Common.Metadata;
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Application.UnitTests.Core.WrittenContentLibrary.BooksLibrary.Books.Commands.AddBook.Fixtures;
using Lumina.Contracts.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Contains unit tests for the <see cref="BookEntityMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookModelMappingTests
{
    private readonly BookEntityFixture _bookEntityFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="BookModelMappingTests"/> class.
    /// </summary>
    public BookModelMappingTests()
    {
        _bookEntityFixture = new BookEntityFixture();
    }

    [Fact]
    public void ToDomainModel_WhenMappingValidBookEntity_ShouldMapCorrectly()
    {
        // Arrange
        BookEntity bookEntity = _bookEntityFixture.CreateBook();

        // Act
        ErrorOr<Book> result = bookEntity.ToDomainEntity();

        // Assert
        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Id.Value.Should().Be(bookEntity.Id);
        result.Value.Metadata.Title.Should().Be(bookEntity.Title);

        if (bookEntity.OriginalTitle is not null)
        {
            result.Value.Metadata.OriginalTitle.HasValue.Should().BeTrue();
            result.Value.Metadata.OriginalTitle.Value.Should().Be(bookEntity.OriginalTitle);
        }
        else
            result.Value.Metadata.OriginalTitle.HasValue.Should().BeFalse();

        if (bookEntity.Description is not null)
        {
            result.Value.Metadata.Description.HasValue.Should().BeTrue();
            result.Value.Metadata.Description.Value.Should().Be(bookEntity.Description);
        }
        else
            result.Value.Metadata.Description.HasValue.Should().BeFalse();
        if (bookEntity.OriginalReleaseDate.HasValue)
        {
            result.Value.Metadata.ReleaseInfo.OriginalReleaseDate.HasValue.Should().BeTrue();
            result.Value.Metadata.ReleaseInfo.OriginalReleaseDate.Value.Should().Be(bookEntity.OriginalReleaseDate.Value);
        }
        else
            result.Value.Metadata.ReleaseInfo.OriginalReleaseDate.HasValue.Should().BeFalse();

        if (bookEntity.OriginalReleaseYear.HasValue)
        {
            result.Value.Metadata.ReleaseInfo.OriginalReleaseYear.HasValue.Should().BeTrue();
            result.Value.Metadata.ReleaseInfo.OriginalReleaseYear.Value.Should().Be(bookEntity.OriginalReleaseYear.Value);
        }
        else
            result.Value.Metadata.ReleaseInfo.OriginalReleaseYear.HasValue.Should().BeFalse();

        if (bookEntity.ReReleaseDate.HasValue)
        {
            result.Value.Metadata.ReleaseInfo.ReReleaseDate.HasValue.Should().BeTrue();
            result.Value.Metadata.ReleaseInfo.ReReleaseDate.Value.Should().Be(bookEntity.ReReleaseDate.Value);
        }
        else
            result.Value.Metadata.ReleaseInfo.ReReleaseDate.HasValue.Should().BeFalse();

        if (bookEntity.ReReleaseYear.HasValue)
        {
            result.Value.Metadata.ReleaseInfo.ReReleaseYear.HasValue.Should().BeTrue();
            result.Value.Metadata.ReleaseInfo.ReReleaseYear.Value.Should().Be(bookEntity.ReReleaseYear.Value);
        }
        else
            result.Value.Metadata.ReleaseInfo.ReReleaseYear.HasValue.Should().BeFalse();

        if (bookEntity.ReleaseCountry is not null)
        {
            result.Value.Metadata.ReleaseInfo.ReleaseCountry.HasValue.Should().BeTrue();
            result.Value.Metadata.ReleaseInfo.ReleaseCountry.Value.Should().Be(bookEntity.ReleaseCountry);
        }
        else
            result.Value.Metadata.ReleaseInfo.ReleaseCountry.HasValue.Should().BeFalse();

        if (bookEntity.ReleaseVersion is not null)
        {
            result.Value.Metadata.ReleaseInfo.ReleaseVersion.HasValue.Should().BeTrue();
            result.Value.Metadata.ReleaseInfo.ReleaseVersion.Value.Should().Be(bookEntity.ReleaseVersion);
        }
        else
            result.Value.Metadata.ReleaseInfo.ReleaseVersion.HasValue.Should().BeFalse();

        if (bookEntity.LanguageCode is not null)
        {
            result.Value.Metadata.Language.HasValue.Should().BeTrue();
            result.Value.Metadata.Language.Value.LanguageCode.Should().Be(bookEntity.LanguageCode);
            result.Value.Metadata.Language.Value.LanguageName.Should().Be(bookEntity.LanguageName);

            if (bookEntity.LanguageNativeName is not null)
            {
                result.Value.Metadata.Language.Value.NativeName.HasValue.Should().BeTrue();
                result.Value.Metadata.Language.Value.NativeName.Value.Should().Be(bookEntity.LanguageNativeName);
            }
            else
                result.Value.Metadata.Language.Value.NativeName.HasValue.Should().BeFalse();
        }
        else
            result.Value.Metadata.Language.HasValue.Should().BeFalse();

        if (bookEntity.OriginalLanguageCode is not null)
        {
            result.Value.Metadata.OriginalLanguage.HasValue.Should().BeTrue();
            result.Value.Metadata.OriginalLanguage.Value.LanguageCode.Should().Be(bookEntity.OriginalLanguageCode);
            result.Value.Metadata.OriginalLanguage.Value.LanguageName.Should().Be(bookEntity.OriginalLanguageName);

            if (bookEntity.OriginalLanguageNativeName is not null)
            {
                result.Value.Metadata.OriginalLanguage.Value.NativeName.HasValue.Should().BeTrue();
                result.Value.Metadata.OriginalLanguage.Value.NativeName.Value.Should().Be(bookEntity.OriginalLanguageNativeName);
            }
            else
                result.Value.Metadata.OriginalLanguage.Value.NativeName.HasValue.Should().BeFalse();
        }
        else
            result.Value.Metadata.OriginalLanguage.HasValue.Should().BeFalse();

        if (bookEntity.Publisher is not null)
        {
            result.Value.Metadata.Publisher.HasValue.Should().BeTrue();
            result.Value.Metadata.Publisher.Value.Should().Be(bookEntity.Publisher);
        }
        else
            result.Value.Metadata.Publisher.HasValue.Should().BeFalse();

        if (bookEntity.PageCount.HasValue)
        {
            result.Value.Metadata.PageCount.HasValue.Should().BeTrue();
            result.Value.Metadata.PageCount.Value.Should().Be(bookEntity.PageCount.Value);
        }
        else
            result.Value.Metadata.PageCount.HasValue.Should().BeFalse();

        result.Value.Format.Value.Should().Be(bookEntity.Format);

        if (bookEntity.Edition is not null)
        {
            result.Value.Edition.HasValue.Should().BeTrue();
            result.Value.Edition.Value.Should().Be(bookEntity.Edition);
        }
        else
            result.Value.Edition.HasValue.Should().BeFalse();

        if (bookEntity.VolumeNumber.HasValue)
        {
            result.Value.VolumeNumber.HasValue.Should().BeTrue();
            result.Value.VolumeNumber.Value.Should().Be(bookEntity.VolumeNumber.Value);
        }
        else
            result.Value.VolumeNumber.HasValue.Should().BeFalse();

        if (bookEntity.ASIN is not null)
        {
            result.Value.ASIN.HasValue.Should().BeTrue();
            result.Value.ASIN.Value.Should().Be(bookEntity.ASIN);
        }
        else
            result.Value.ASIN.HasValue.Should().BeFalse();

        if (bookEntity.GoodreadsId is not null)
        {
            result.Value.GoodreadsId.HasValue.Should().BeTrue();
            result.Value.GoodreadsId.Value.Should().Be(bookEntity.GoodreadsId);
        }
        else
            result.Value.GoodreadsId.HasValue.Should().BeFalse();

        if (bookEntity.LCCN is not null)
        {
            result.Value.LCCN.HasValue.Should().BeTrue();
            result.Value.LCCN.Value.Should().Be(bookEntity.LCCN);
        }
        else
            result.Value.LCCN.HasValue.Should().BeFalse();

        if (bookEntity.OCLCNumber is not null)
        {
            result.Value.OCLCNumber.HasValue.Should().BeTrue();
            result.Value.OCLCNumber.Value.Should().Be(bookEntity.OCLCNumber);
        }
        else
            result.Value.OCLCNumber.HasValue.Should().BeFalse();

        if (bookEntity.OpenLibraryId is not null)
        {
            result.Value.OpenLibraryId.HasValue.Should().BeTrue();
            result.Value.OpenLibraryId.Value.Should().Be(bookEntity.OpenLibraryId);
        }
        else
            result.Value.OpenLibraryId.HasValue.Should().BeFalse();

        if (bookEntity.LibraryThingId is not null)
        {
            result.Value.LibraryThingId.HasValue.Should().BeTrue();
            result.Value.LibraryThingId.Value.Should().Be(bookEntity.LibraryThingId);
        }
        else
            result.Value.LibraryThingId.HasValue.Should().BeFalse();

        if (bookEntity.GoogleBooksId is not null)
        {
            result.Value.GoogleBooksId.HasValue.Should().BeTrue();
            result.Value.GoogleBooksId.Value.Should().Be(bookEntity.GoogleBooksId);
        }
        else
            result.Value.GoogleBooksId.HasValue.Should().BeFalse();

        if (bookEntity.BarnesAndNobleId is not null)
        {
            result.Value.BarnesAndNobleId.HasValue.Should().BeTrue();
            result.Value.BarnesAndNobleId.Value.Should().Be(bookEntity.BarnesAndNobleId);
        }
        else
            result.Value.BarnesAndNobleId.HasValue.Should().BeFalse();

        if (bookEntity.AppleBooksId is not null)
        {
            result.Value.AppleBooksId.HasValue.Should().BeTrue();
            result.Value.AppleBooksId.Value.Should().Be(bookEntity.AppleBooksId);
        }
        else
            result.Value.AppleBooksId.HasValue.Should().BeFalse();

        result.Value.Created.Should().Be(bookEntity.Created);

        if (bookEntity.Updated.HasValue)
        {
            result.Value.Updated.HasValue.Should().BeTrue();
            result.Value.Updated!.Value.Should().Be(bookEntity.Updated.Value);
        }
        else
            result.Value.Updated.HasValue.Should().BeFalse();
    }

    [Fact]
    public void ToResponse_WhenMappingBookEntity_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        BookEntity bookEntity = _bookEntityFixture.CreateBook();

        // Act
        BookResponse result = bookEntity.ToResponse();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(bookEntity.Id);
        result.Metadata.Title.Should().Be(bookEntity.Title);
        result.Metadata.OriginalTitle.Should().Be(bookEntity.OriginalTitle);
        result.Metadata.Description.Should().Be(bookEntity.Description);
        result.Metadata.ReleaseInfo!.OriginalReleaseDate.Should().Be(bookEntity.OriginalReleaseDate);
        result.Metadata.ReleaseInfo.OriginalReleaseYear.Should().Be(bookEntity.OriginalReleaseYear);
        result.Metadata.ReleaseInfo.ReReleaseDate.Should().Be(bookEntity.ReReleaseDate);
        result.Metadata.ReleaseInfo.ReReleaseYear.Should().Be(bookEntity.ReReleaseYear);
        result.Metadata.ReleaseInfo.ReleaseCountry.Should().Be(bookEntity.ReleaseCountry);
        result.Metadata.ReleaseInfo.ReleaseVersion.Should().Be(bookEntity.ReleaseVersion);
        result.Metadata.Language.Should().NotBeNull();
        result.Metadata.Language!.LanguageCode.Should().Be(bookEntity.LanguageCode);
        result.Metadata.Language.LanguageName.Should().Be(bookEntity.LanguageName);
        result.Metadata.Language.NativeName.Should().Be(bookEntity.LanguageNativeName);
        result.Metadata.OriginalLanguage.Should().NotBeNull();
        result.Metadata.OriginalLanguage!.LanguageCode.Should().Be(bookEntity.OriginalLanguageCode);
        result.Metadata.OriginalLanguage.LanguageName.Should().Be(bookEntity.OriginalLanguageName);
        result.Metadata.OriginalLanguage.NativeName.Should().Be(bookEntity.OriginalLanguageNativeName);
        result.Metadata.Publisher.Should().Be(bookEntity.Publisher);
        result.Metadata.PageCount.Should().Be(bookEntity.PageCount);
        result.Format.Should().Be(bookEntity.Format);
        result.Edition.Should().Be(bookEntity.Edition);
        result.VolumeNumber.Should().Be(bookEntity.VolumeNumber);
        result.ASIN.Should().Be(bookEntity.ASIN);
        result.GoodreadsId.Should().Be(bookEntity.GoodreadsId);
        result.LCCN.Should().Be(bookEntity.LCCN);
        result.OCLCNumber.Should().Be(bookEntity.OCLCNumber);
        result.OpenLibraryId.Should().Be(bookEntity.OpenLibraryId);
        result.LibraryThingId.Should().Be(bookEntity.LibraryThingId);
        result.GoogleBooksId.Should().Be(bookEntity.GoogleBooksId);
        result.BarnesAndNobleId.Should().Be(bookEntity.BarnesAndNobleId);
        result.AppleBooksId.Should().Be(bookEntity.AppleBooksId);
        result.ISBNs.Should().BeEquivalentTo(bookEntity.ISBNs);
        result.Ratings.Should().BeEquivalentTo(bookEntity.Ratings);
        result.Created.Should().Be(bookEntity.Created);
        result.Updated.Should().Be(bookEntity.Updated);
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
        results.Should().NotBeNull();
        results.Should().HaveCount(bookEntities.Count);
        results.Should().AllSatisfy(result => result.IsError.Should().BeFalse());
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
        results.Should().NotBeNull();
        results.Should().HaveCount(bookEntities.Count);
    }
}
