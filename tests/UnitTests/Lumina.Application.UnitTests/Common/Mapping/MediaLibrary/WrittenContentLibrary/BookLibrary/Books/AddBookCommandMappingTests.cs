#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.AddBook;
using Lumina.Application.Fixtures.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.AddBook;
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
/// Contains unit tests for the <see cref="AddBookCommandMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddBookCommandMappingTests
{
    private readonly AddBookCommandFixture _addBookCommandFixture = new();
    private readonly IsbnDtoFixture _isbnDtoFixture = new();
    private readonly BookRatingDtoFixture _bookRatingDtoFixture = new();
    private readonly GenreDtoFixture _genreDtoFixture = new();
    private readonly WrittenContentMetadataDtoFixture _writtenContentMetadataDtoFixture = new();

    [Fact]
    public void ToDomainEntity_WhenMappingCompleteCommand_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        AddBookCommand command = _addBookCommandFixture.Create(libraryId: libraryId.ToString());

        // Act
        Result<Book> result = command.ToDomainEntity(libraryId);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(libraryId, result.Value.LibraryId.Value);
        Assert.Equal(command.Path, result.Value.Path);
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
        AddBookCommand command = _addBookCommandFixture.Create(
            libraryId: libraryId.ToString(),
            isbns: [],
            contributors: [],
            ratings: [],
            includeFormat: false,
            includeEdition: false,
            includeVolumeNumber: false,
            includeAsin: false,
            includeGoodreadsId: false,
            includeLccn: false,
            includeOclcNumber: false,
            includeOpenLibraryId: false,
            includeLibraryThingId: false,
            includeGoogleBooksId: false,
            includeBarnesAndNobleId: false,
            includeAppleBooksId: false);

        // Act
        Result<Book> result = command.ToDomainEntity(libraryId);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(libraryId, result.Value.LibraryId.Value);
        Assert.Equal(command.Path, result.Value.Path);
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
        AddBookCommand command = _addBookCommandFixture.Create(ratings: [_bookRatingDtoFixture.Create(value: -1, maxValue: 5, includeSource: false, includeVoteCount: false)]);

        // Act
        Result<Book> result = command.ToDomainEntity(Guid.NewGuid());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, error => error.Description == Errors.Metadata.RatingValueMustBePositive.Description);
    }

    [Fact]
    public void ToDomainEntity_WhenIsbnCreationFails_ShouldReturnError()
    {
        // Arrange
        AddBookCommand command = _addBookCommandFixture.Create(isbns: [_isbnDtoFixture.Create(value: "invalid", format: IsbnFormat.Isbn13)]);

        // Act
        Result<Book> result = command.ToDomainEntity(Guid.NewGuid());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, error => error.Description == Errors.WrittenContent.InvalidIsbn13Format.Description);
    }

    [Fact]
    public void ToDomainEntity_WhenMetadataCreationFails_ShouldReturnError()
    {
        // Arrange
        AddBookCommand command = _addBookCommandFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(genres: [_genreDtoFixture.Create(name: "")]));

        // Act
        Result<Book> result = command.ToDomainEntity(Guid.NewGuid());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, error => error.Description == Errors.Metadata.GenreNameCannotBeEmpty.Description);
    }
}
