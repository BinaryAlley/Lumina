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
            goodreadsId: "3") with
        {
            Genres = [_genreDtoFixture.Create(name: "")]
        };

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
        BookMetadataDto metadata = _bookMetadataDtoFixture.Create(title: title, includeOptionalProperties: false);

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
        BookMetadataDto metadata = _bookMetadataDtoFixture.Create(title: "A valid title", includeOptionalProperties: false);

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
            releaseInfo: new ReleaseInfoDto(new DateOnly(2000, 1, 1), 1999, null, null, null, null));

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
            tags: [new TagDto("   ")]);

        // Act
        Result<Success> result = book.ApplyMetadata(metadata);

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void ApplyMetadata_WhenLanguageIsInvalid_ShouldReturnError()
    {
        // Arrange
        Book book = _bookFixture.Create();
        BookMetadataDto metadata = _bookMetadataDtoFixture.Create(
            title: "A valid title",
            language: new LanguageInfoDto("", "English", "English"));

        // Act
        Result<Success> result = book.ApplyMetadata(metadata);

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void ApplyMetadata_WhenOriginalLanguageIsInvalid_ShouldReturnError()
    {
        // Arrange
        Book book = _bookFixture.Create();
        BookMetadataDto metadata = _bookMetadataDtoFixture.Create(
            title: "A valid title",
            originalLanguage: new LanguageInfoDto("", "English", "English"));

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
            isbns: [new IsbnDto("not-an-isbn", IsbnFormat.Isbn13)]);

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
            ratings: [new BookRatingDto(-1m, 5m, null, null)]);

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
            releaseInfo: new ReleaseInfoDto(new DateOnly(2000, 1, 1), 2000, null, null, null, null),
            includeOptionalProperties: false);

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
            isbns: [new IsbnDto("9780306406157", IsbnFormat.Isbn13)],
            ratings: [new BookRatingDto(4m, 5m, null, 10)]);

        // Act
        Result<Success> result = book.ApplyMetadata(metadata);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Single(book.ISBNs);
        Assert.Single(book.Ratings);
    }
}
