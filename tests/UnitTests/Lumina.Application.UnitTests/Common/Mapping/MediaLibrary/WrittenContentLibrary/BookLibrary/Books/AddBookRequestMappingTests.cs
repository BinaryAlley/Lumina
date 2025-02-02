#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BooksLibrary.Books.Commands.AddBook;
using Lumina.Application.UnitTests.Core.MediaLibrary.WrittenContentLibrary.BooksLibrary.Books.Commands.AddBook.Fixtures;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Domain.Common.Enums.BookLibrary;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Contains unit tests for the <see cref="AddBookRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddBookRequestMappingTests
{
    private readonly AddBookRequestFixture _requestFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddBookRequestMappingTests"/> class.
    /// </summary>
    public AddBookRequestMappingTests()
    {
        _requestFixture = new AddBookRequestFixture();
    }

    [Fact]
    public void ToCommand_WhenMappingCompleteRequest_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        AddBookRequest request = _requestFixture.CreateRequestBook();

        // Act
        AddBookCommand result = request.ToCommand();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Metadata, result.Metadata);
        Assert.Equal(request.Format, result.Format);
        Assert.Equal(request.Edition, result.Edition);
        Assert.Equal(request.VolumeNumber, result.VolumeNumber);
        Assert.Equal(request.Series, result.Series);
        Assert.Equal(request.ASIN, result.ASIN);
        Assert.Equal(request.GoodreadsId, result.GoodreadsId);
        Assert.Equal(request.LCCN, result.LCCN);
        Assert.Equal(request.OCLCNumber, result.OCLCNumber);
        Assert.Equal(request.OpenLibraryId, result.OpenLibraryId);
        Assert.Equal(request.LibraryThingId, result.LibraryThingId);
        Assert.Equal(request.GoogleBooksId, result.GoogleBooksId);
        Assert.Equal(request.BarnesAndNobleId, result.BarnesAndNobleId);
        Assert.Equal(request.AppleBooksId, result.AppleBooksId);
        Assert.Equal(request.ISBNs, result.ISBNs);
        Assert.Equal(request.Contributors, result.Contributors);
        Assert.Equal(request.Ratings, result.Ratings);
    }

    [Fact]
    public void ToCommand_WhenMappingMinimalRequest_ShouldMapCorrectly()
    {
        // Arrange
        AddBookRequest request = new(
            Metadata: new WrittenContentMetadataDto(
                Title: "Test Book",
                OriginalTitle: null,
                Description: null,
                ReleaseInfo: null,
                Genres: null,
                Tags: null,
                Language: null,
                OriginalLanguage: null,
                Publisher: null,
                PageCount: null),
            Format: null,
            Edition: null,
            VolumeNumber: null,
            Series: null,
            ASIN: null,
            GoodreadsId: null,
            LCCN: null,
            OCLCNumber: null,
            OpenLibraryId: null,
            LibraryThingId: null,
            GoogleBooksId: null,
            BarnesAndNobleId: null,
            AppleBooksId: null,
            ISBNs: null,
            Contributors: null,
            Ratings: null);

        // Act
        AddBookCommand result = request.ToCommand();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Metadata, result.Metadata);
        Assert.Null(result.Format);
        Assert.Null(result.Edition);
        Assert.Null(result.VolumeNumber);
        Assert.Null(result.Series);
        Assert.Null(result.ASIN);
        Assert.Null(result.GoodreadsId);
        Assert.Null(result.LCCN);
        Assert.Null(result.OCLCNumber);
        Assert.Null(result.OpenLibraryId);
        Assert.Null(result.LibraryThingId);
        Assert.Null(result.GoogleBooksId);
        Assert.Null(result.BarnesAndNobleId);
        Assert.Null(result.AppleBooksId);
        Assert.Null(result.ISBNs);
        Assert.Null(result.Contributors);
        Assert.Null(result.Ratings);
    }

    [Fact]
    public void ToCommand_WhenMappingRequestWithCollections_ShouldMapCollectionsCorrectly()
    {
        // Arrange
        AddBookRequest request = new(
            Metadata: new WrittenContentMetadataDto(
                Title: "Test Book",
                OriginalTitle: null,
                Description: null,
                ReleaseInfo: null,
                Genres: null,
                Tags: null,
                Language: null,
                OriginalLanguage: null,
                Publisher: null,
                PageCount: null),
            Format: null,
            Edition: null,
            VolumeNumber: null,
            Series: null,
            ASIN: null,
            GoodreadsId: null,
            LCCN: null,
            OCLCNumber: null,
            OpenLibraryId: null,
            LibraryThingId: null,
            GoogleBooksId: null,
            BarnesAndNobleId: null,
            AppleBooksId: null,
            ISBNs: [new("978-0-123456-78-9", IsbnFormat.Isbn13)],
            Contributors:
            [
                new(
                    new MediaContributorNameDto("John Doe", "John Smith Doe"),
                    new MediaContributorRoleDto("Author", "Writing")
                )
            ],
            Ratings: [new(4.5m, 5m, BookRatingSource.Goodreads, 1000)]);

        // Act
        AddBookCommand result = request.ToCommand();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.ISBNs, result.ISBNs);
        Assert.Equal(request.Contributors, result.Contributors);
        Assert.Equal(request.Ratings, result.Ratings);
    }
}
