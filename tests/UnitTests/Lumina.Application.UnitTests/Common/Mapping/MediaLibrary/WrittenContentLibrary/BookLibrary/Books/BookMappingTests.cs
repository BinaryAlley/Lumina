#region ========================================================================= USING =====================================================================================
using FluentAssertions;
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
        result.Should().NotBeNull();
        result.Id.Should().Be(book.Id.Value);
        result.Title.Should().Be(book.Metadata.Title);
        result.OriginalTitle.Should().Be(book.Metadata.OriginalTitle.HasValue ? book.Metadata.OriginalTitle.Value : null);
        result.Description.Should().Be(book.Metadata.Description.HasValue ? book.Metadata.Description.Value : null);
        result.OriginalReleaseDate.Should().Be(book.Metadata.ReleaseInfo.OriginalReleaseDate.HasValue ? book.Metadata.ReleaseInfo.OriginalReleaseDate.Value : null);
        result.OriginalReleaseYear.Should().Be(book.Metadata.ReleaseInfo.OriginalReleaseYear.HasValue ? book.Metadata.ReleaseInfo.OriginalReleaseYear.Value : null);
        result.ReReleaseDate.Should().Be(book.Metadata.ReleaseInfo.ReReleaseDate.HasValue ? book.Metadata.ReleaseInfo.ReReleaseDate.Value : null);
        result.ReReleaseYear.Should().Be(book.Metadata.ReleaseInfo.ReReleaseYear.HasValue ? book.Metadata.ReleaseInfo.ReReleaseYear.Value : null);
        result.ReleaseCountry.Should().Be(book.Metadata.ReleaseInfo.ReleaseCountry.HasValue ? book.Metadata.ReleaseInfo.ReleaseCountry.Value : null);
        result.ReleaseVersion.Should().Be(book.Metadata.ReleaseInfo.ReleaseVersion.HasValue ? book.Metadata.ReleaseInfo.ReleaseVersion.Value : null);
        result.LanguageCode.Should().Be(book.Metadata.Language.HasValue ? book.Metadata.Language.Value.LanguageCode : null);
        result.LanguageName.Should().Be(book.Metadata.Language.HasValue ? book.Metadata.Language.Value.LanguageName : null);
        result.LanguageNativeName.Should().Be(book.Metadata.Language.HasValue ? book.Metadata.Language.Value.NativeName.Value : null);
        result.OriginalLanguageCode.Should().Be(book.Metadata.OriginalLanguage.HasValue ? book.Metadata.OriginalLanguage.Value.LanguageCode : null);
        result.OriginalLanguageName.Should().Be(book.Metadata.OriginalLanguage.HasValue ? book.Metadata.OriginalLanguage.Value.LanguageName : null);
        result.OriginalLanguageNativeName.Should().Be(book.Metadata.OriginalLanguage.HasValue ? book.Metadata.OriginalLanguage.Value.NativeName.Value : null);
        result.Tags.Should().BeEquivalentTo(book.Metadata.Tags.ToRepositoryEntities());
        result.Genres.Should().BeEquivalentTo(book.Metadata.Genres.ToRepositoryEntities());
        result.Publisher.Should().Be(book.Metadata.Publisher.HasValue ? book.Metadata.Publisher.Value : null);
        result.PageCount.Should().Be(book.Metadata.PageCount.HasValue ? book.Metadata.PageCount.Value : null);
        result.Format.Should().Be(book.Format.HasValue ? book.Format.Value : null);
        result.Edition.Should().Be(book.Edition.HasValue ? book.Edition.Value : null);
        result.VolumeNumber.Should().Be(book.VolumeNumber.HasValue ? book.VolumeNumber.Value : null);
        result.ASIN.Should().Be(book.ASIN.HasValue ? book.ASIN.Value : null);
        result.GoodreadsId.Should().Be(book.GoodreadsId.HasValue ? book.GoodreadsId.Value : null);
        result.LCCN.Should().Be(book.LCCN.HasValue ? book.LCCN.Value : null);
        result.OCLCNumber.Should().Be(book.OCLCNumber.HasValue ? book.OCLCNumber.Value : null);
        result.OpenLibraryId.Should().Be(book.OpenLibraryId.HasValue ? book.OpenLibraryId.Value : null);
        result.LibraryThingId.Should().Be(book.LibraryThingId.HasValue ? book.LibraryThingId.Value : null);
        result.GoogleBooksId.Should().Be(book.GoogleBooksId.HasValue ? book.GoogleBooksId.Value : null);
        result.BarnesAndNobleId.Should().Be(book.BarnesAndNobleId.HasValue ? book.BarnesAndNobleId.Value : null);
        result.AppleBooksId.Should().Be(book.AppleBooksId.HasValue ? book.AppleBooksId.Value : null);
        result.ISBNs.Should().BeEquivalentTo(book.ISBNs.ToRepositoryEntities());
        result.Ratings.Should().BeEquivalentTo(book.Ratings.ToRepositoryEntities());
        result.Created.Should().Be(book.Created);
        result.Updated.Should().Be(book.Updated.HasValue ? book.Updated.Value : null);
    }
}
