#region ========================================================================= USING =====================================================================================
using FluentAssertions;
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
        result.Should().NotBeNull();
        result.Metadata.Should().BeEquivalentTo(request.Metadata);
        result.Format.Should().Be(request.Format);
        result.Edition.Should().Be(request.Edition);
        result.VolumeNumber.Should().Be(request.VolumeNumber);
        result.Series.Should().BeEquivalentTo(request.Series);
        result.ASIN.Should().Be(request.ASIN);
        result.GoodreadsId.Should().Be(request.GoodreadsId);
        result.LCCN.Should().Be(request.LCCN);
        result.OCLCNumber.Should().Be(request.OCLCNumber);
        result.OpenLibraryId.Should().Be(request.OpenLibraryId);
        result.LibraryThingId.Should().Be(request.LibraryThingId);
        result.GoogleBooksId.Should().Be(request.GoogleBooksId);
        result.BarnesAndNobleId.Should().Be(request.BarnesAndNobleId);
        result.AppleBooksId.Should().Be(request.AppleBooksId);
        result.ISBNs.Should().BeEquivalentTo(request.ISBNs);
        result.Contributors.Should().BeEquivalentTo(request.Contributors);
        result.Ratings.Should().BeEquivalentTo(request.Ratings);
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
        result.Should().NotBeNull();
        result.Metadata.Should().BeEquivalentTo(request.Metadata);
        result.Format.Should().BeNull();
        result.Edition.Should().BeNull();
        result.VolumeNumber.Should().BeNull();
        result.Series.Should().BeNull();
        result.ASIN.Should().BeNull();
        result.GoodreadsId.Should().BeNull();
        result.LCCN.Should().BeNull();
        result.OCLCNumber.Should().BeNull();
        result.OpenLibraryId.Should().BeNull();
        result.LibraryThingId.Should().BeNull();
        result.GoogleBooksId.Should().BeNull();
        result.BarnesAndNobleId.Should().BeNull();
        result.AppleBooksId.Should().BeNull();
        result.ISBNs.Should().BeNull();
        result.Contributors.Should().BeNull();
        result.Ratings.Should().BeNull();
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
        result.Should().NotBeNull();
        result.ISBNs.Should().BeEquivalentTo(request.ISBNs);
        result.Contributors.Should().BeEquivalentTo(request.Contributors);
        result.Ratings.Should().BeEquivalentTo(request.Ratings);
    }
}
