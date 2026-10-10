#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.Common;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Contains unit tests for the <see cref="BookMetadataDtoMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookMetadataDtoMappingTests
{
    private readonly BookFixture _bookFixture = new();
    private readonly BookMetadataDtoFixture _bookMetadataDtoFixture = new();
    private readonly GenreDtoFixture _genreDtoFixture = new();
    private readonly ReleaseInfoDtoFixture _releaseInfoDtoFixture = new();
    private readonly TagDtoFixture _tagDtoFixture = new();
    private readonly IsbnDtoFixture _isbnDtoFixture = new();
    private readonly BookRatingDtoFixture _bookRatingDtoFixture = new();

    [Fact]
    public void ApplyMetadata_WhenCalledWithValidMetadata_ShouldApplyItToTheBook()
    {
        // Arrange
        Book book = _bookFixture.Create();
        BookMetadataDto metadata = _bookMetadataDtoFixture.Create(
            title: "The Fellowship of the Ring",
            description: "The first part of J.R.R. Tolkien's epic adventure.",
            goodreadsId: "3",
            format: BookFormat.Paperback,
            publisher: "Houghton Mifflin",
            pageCount: 398);

        // Act
        Result<Success> result = book.ApplyMetadata(metadata);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal("The Fellowship of the Ring", book.Metadata.Title);
        Assert.Equal("3", book.GoodreadsId.Value);
        Assert.Equal("The first part of J.R.R. Tolkien's epic adventure.", book.Metadata.Description.Value);
        Assert.Equal(BookFormat.Paperback, book.Format.Value);
        Assert.Equal("Houghton Mifflin", book.Metadata.Publisher.Value);
    }

    [Fact]
    public void ApplyMetadata_WhenCalledWithInvalidGenres_ShouldReturnError()
    {
        // Arrange
        Book book = _bookFixture.Create();
        BookMetadataDto metadata = _bookMetadataDtoFixture.Create(
            title: "The Fellowship of the Ring",
            goodreadsId: "3",
            genres: [_genreDtoFixture.Create(name: "")]);

        // Act
        Result<Success> result = book.ApplyMetadata(metadata);

        // Assert
        Assert.True(result.IsFailure);
        Assert.NotEqual("The Fellowship of the Ring", book.Metadata.Title);
    }

    [Theory]
    [InlineData(null)] // missing title
    [InlineData("")] // empty title
    [InlineData("   ")] // whitespace title
    public void ApplyMetadata_WhenTitleIsNullOrWhitespace_ShouldReturnTitleCannotBeEmptyError(string? title)
    {
        // Arrange
        Book book = _bookFixture.Create();
        BookMetadataDto metadata = _bookMetadataDtoFixture.Create(includeTitle: title is not null, title: title, includeReleaseInfo: false, includeGenres: false, includeTags: false, includeLanguage: false, includeFormat: false, includeVolumeNumber: false);

        // Act
        Result<Success> result = book.ApplyMetadata(metadata);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Metadata.TitleCannotBeEmpty, result.FirstError);
    }

    [Fact]
    public void ApplyMetadata_WhenReleaseInfoIsNull_ShouldReturnReleaseInfoCannotBeNullError()
    {
        // Arrange
        Book book = _bookFixture.Create();
        BookMetadataDto metadata = _bookMetadataDtoFixture.Create(title: "A valid title", includeReleaseInfo: false, includeGenres: false, includeTags: false, includeLanguage: false, includeFormat: false, includeVolumeNumber: false);

        // Act
        Result<Success> result = book.ApplyMetadata(metadata);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Metadata.ReleaseInfoCannotBeNull, result.FirstError);
    }

    [Fact]
    public void ApplyMetadata_WhenReleaseInfoIsInvalid_ShouldReturnError()
    {
        // Arrange
        Book book = _bookFixture.Create();
        BookMetadataDto metadata = _bookMetadataDtoFixture.Create(
            title: "A valid title",
            releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseDate: new DateOnly(2000, 1, 1), originalReleaseYear: 1999));

        // Act
        Result<Success> result = book.ApplyMetadata(metadata);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Metadata.OriginalReleaseDateAndYearMustMatch, result.FirstError);
    }

    [Fact]
    public void ApplyMetadata_WhenTagsAreInvalid_ShouldReturnError()
    {
        // Arrange
        Book book = _bookFixture.Create();
        BookMetadataDto metadata = _bookMetadataDtoFixture.Create(
            title: "A valid title",
            tags: [_tagDtoFixture.Create(name: "   ")]);

        // Act
        Result<Success> result = book.ApplyMetadata(metadata);

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void ApplyMetadata_WhenIsbnIsInvalid_ShouldReturnError()
    {
        // Arrange
        Book book = _bookFixture.Create();
        BookMetadataDto metadata = _bookMetadataDtoFixture.Create(
            title: "A valid title",
            isbns: [_isbnDtoFixture.Create(value: "not-an-isbn", format: IsbnFormat.Isbn13)]);

        // Act
        Result<Success> result = book.ApplyMetadata(metadata);

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void ApplyMetadata_WhenRatingIsInvalid_ShouldReturnError()
    {
        // Arrange
        Book book = _bookFixture.Create();
        BookMetadataDto metadata = _bookMetadataDtoFixture.Create(
            title: "A valid title",
            ratings: [_bookRatingDtoFixture.Create(value: -1m, maxValue: 5m, includeSource: false, includeVoteCount: false)]);

        // Act
        Result<Success> result = book.ApplyMetadata(metadata);

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void ApplyMetadata_WhenOptionalCollectionsAreNull_ShouldApplyMetadataWithoutCollections()
    {
        // Arrange
        Book book = _bookFixture.Create();
        BookMetadataDto metadata = _bookMetadataDtoFixture.Create(
            title: "A valid title",
            releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseDate: new DateOnly(2000, 1, 1), originalReleaseYear: 2000),
            includeGenres: false,
            includeTags: false,
            includeLanguage: false,
            includeFormat: false,
            includeVolumeNumber: false);

        // Act
        Result<Success> result = book.ApplyMetadata(metadata);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal("A valid title", book.Metadata.Title);
    }

    [Fact]
    public void ApplyMetadata_WhenIsbnsAndRatingsAreValid_ShouldApplyMetadataWithTheCollections()
    {
        // Arrange
        Book book = _bookFixture.Create();
        BookMetadataDto metadata = _bookMetadataDtoFixture.Create(
            title: "A valid title",
            isbns: [_isbnDtoFixture.Create(value: "9780306406157", format: IsbnFormat.Isbn13)],
            ratings: [_bookRatingDtoFixture.Create(value: 4m, maxValue: 5m, voteCount: 10, includeSource: false)]);

        // Act
        Result<Success> result = book.ApplyMetadata(metadata);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Single(book.ISBNs);
        Assert.Single(book.Ratings);
    }
}
