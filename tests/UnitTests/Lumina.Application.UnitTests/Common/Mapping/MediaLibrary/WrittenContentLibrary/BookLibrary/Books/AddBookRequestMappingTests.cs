#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BooksLibrary.Books.Commands.AddBook;
using Lumina.Contracts.Fixtures.Core.DTO.MediaContributors;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Contains unit tests for the <see cref="AddBookRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddBookRequestMappingTests
{
    private readonly AddBookRequestFixture _requestFixture = new();
    private readonly WrittenContentMetadataDtoFixture _writtenContentMetadataDtoFixture = new();
    private readonly MediaContributorNameDtoFixture _mediaContributorNameDtoFixture = new();
    private readonly MediaContributorRoleDtoFixture _mediaContributorRoleDtoFixture = new();

    [Fact]
    public void ToCommand_WhenMappingCompleteRequest_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        AddBookRequest request = _requestFixture.Create();

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
        AddBookRequest request = _requestFixture.Create(
            libraryId: Guid.NewGuid(),
            path: "/books/test.epub",
            metadata: _writtenContentMetadataDtoFixture.Create(title: "Test Book"),
            includeOptionalProperties: false
        );

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
        AddBookRequest request = _requestFixture.Create(
            libraryId: Guid.NewGuid(),
            path: "/books/test.epub",
            metadata: _writtenContentMetadataDtoFixture.Create(title: "Test Book"),
            isbns: [new("978-0-123456-78-9", IsbnFormat.Isbn13)],
            contributors:
            [
                new(
                    _mediaContributorNameDtoFixture.Create(displayName: "John Doe", legalName: "John Smith Doe"),
                    _mediaContributorRoleDtoFixture.Create(name: "Author", category: MediaContributorRoleCategory.Author)
                )
            ],
            ratings: [new(4.5m, 5m, BookRatingSource.Goodreads, 1000)],
            includeOptionalProperties: false
        );

        // Act
        AddBookCommand result = request.ToCommand();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.ISBNs, result.ISBNs);
        Assert.Equal(request.Contributors, result.Contributors);
        Assert.Equal(request.Ratings, result.Ratings);
    }
}
