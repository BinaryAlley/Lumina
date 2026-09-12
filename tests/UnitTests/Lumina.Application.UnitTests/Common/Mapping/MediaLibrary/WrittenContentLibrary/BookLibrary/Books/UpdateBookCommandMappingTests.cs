#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.UpdateBook;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Fixtures.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.UpdateBook;
using Lumina.Contracts.Fixtures.Core.DTO.Common;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Contains unit tests for the <see cref="UpdateBookCommandMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateBookCommandMappingTests
{
    private readonly UpdateBookCommandFixture _commandBookFixture = new();
    private readonly BookEntityFixture _bookEntityFixture = new();
    private readonly IsbnDtoFixture _isbnDtoFixture = new();
    private readonly BookRatingDtoFixture _bookRatingDtoFixture = new();
    private readonly GenreDtoFixture _genreDtoFixture = new();
    private readonly WrittenContentMetadataDtoFixture _writtenContentMetadataDtoFixture = new();

    [Fact]
    public void ToDomainEntity_WhenMappingCompleteCommand_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid bookId = Guid.NewGuid();
        UpdateBookCommand command = _commandBookFixture.Create(libraryId: libraryId.ToString(), bookId: bookId.ToString());
        BookEntity existingBook = _bookEntityFixture.Create(id: bookId, libraryId: libraryId, path: "/books/the-book.epub");
        DateTime createdOnUtc = existingBook.CreatedOnUtc;

        // Act
        Result<Book> result = command.ToDomainEntity(existingBook);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(bookId, result.Value.Id.Value);
        Assert.Equal(libraryId, result.Value.LibraryId.Value);
        Assert.Equal(existingBook.Path, result.Value.Path);
        Assert.Equal(createdOnUtc, result.Value.CreatedOnUtc);
        Assert.Equal(command.Metadata!.Title, result.Value.Metadata.Title);
        Assert.Equal(command.Metadata.ReleaseInfo!.OriginalReleaseYear, result.Value.Metadata.ReleaseInfo.OriginalReleaseYear.Value);
        Assert.Equal(command.Metadata.Genres!.Count, result.Value.Metadata.Genres.Count);
        Assert.Equal(command.Metadata.Tags!.Count, result.Value.Metadata.Tags.Count);
        Assert.Equal(command.ISBNs!.Count, result.Value.ISBNs.Count);
        Assert.Equal(command.Contributors!.Count, result.Value.Contributors.Count);
        Assert.Equal(command.Ratings!.Count, result.Value.Ratings.Count);
        Assert.True(result.Value.Format.HasValue);
        Assert.Equal(command.Format, result.Value.Format.Value);
    }

    [Fact]
    public void ToDomainEntity_WhenOptionalPropertiesAreMissing_ShouldMapRequiredPropertiesCorrectly()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid bookId = Guid.NewGuid();
        UpdateBookCommand command = _commandBookFixture.Create(
            libraryId: libraryId.ToString(),
            bookId: bookId.ToString(),
            isbns: [],
            contributors: [],
            ratings: [],
            includeOptionalProperties: false);
        BookEntity existingBook = _bookEntityFixture.Create(id: bookId, libraryId: libraryId, path: "/books/the-book.epub");

        // Act
        Result<Book> result = command.ToDomainEntity(existingBook);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(bookId, result.Value.Id.Value);
        Assert.Equal(libraryId, result.Value.LibraryId.Value);
        Assert.Equal(existingBook.Path, result.Value.Path);
        Assert.Empty(result.Value.ISBNs);
        Assert.Empty(result.Value.Contributors);
        Assert.Empty(result.Value.Ratings);
        Assert.False(result.Value.Format.HasValue);
        Assert.False(result.Value.Edition.HasValue);
        Assert.False(result.Value.VolumeNumber.HasValue);
        Assert.False(result.Value.ASIN.HasValue);
        Assert.False(result.Value.GoodreadsId.HasValue);
        Assert.False(result.Value.LCCN.HasValue);
        Assert.False(result.Value.OCLCNumber.HasValue);
        Assert.False(result.Value.OpenLibraryId.HasValue);
        Assert.False(result.Value.LibraryThingId.HasValue);
        Assert.False(result.Value.GoogleBooksId.HasValue);
        Assert.False(result.Value.BarnesAndNobleId.HasValue);
        Assert.False(result.Value.AppleBooksId.HasValue);
    }

    [Fact]
    public void ToDomainEntity_WhenRatingCreationFails_ShouldReturnError()
    {
        // Arrange
        UpdateBookCommand command = _commandBookFixture.Create(ratings: [_bookRatingDtoFixture.Create(value: -1, maxValue: 5, includeOptionalProperties: false)]);
        BookEntity existingBook = _bookEntityFixture.Create();

        // Act
        Result<Book> result = command.ToDomainEntity(existingBook);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, error => error.Description == Errors.Metadata.RatingValueMustBePositive.Description);
    }

    [Fact]
    public void ToDomainEntity_WhenIsbnCreationFails_ShouldReturnError()
    {
        // Arrange
        UpdateBookCommand command = _commandBookFixture.Create(isbns: [_isbnDtoFixture.Create(value: "invalid", format: IsbnFormat.Isbn13)]);
        BookEntity existingBook = _bookEntityFixture.Create();

        // Act
        Result<Book> result = command.ToDomainEntity(existingBook);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, error => error.Description == Errors.WrittenContent.InvalidIsbn13Format.Description);
    }

    [Fact]
    public void ToDomainEntity_WhenMetadataCreationFails_ShouldReturnError()
    {
        // Arrange
        UpdateBookCommand command = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(genres: [_genreDtoFixture.Create(name: "")]));
        BookEntity existingBook = _bookEntityFixture.Create();

        // Act
        Result<Book> result = command.ToDomainEntity(existingBook);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, error => error.Description == Errors.Metadata.GenreNameCannotBeEmpty.Description);
    }
}
