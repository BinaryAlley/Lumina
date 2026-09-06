#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.UpdateBook;
using Lumina.Application.Fixtures.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.UpdateBook;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Contracts.Fixtures.Core.DTO.Common;
using Lumina.Contracts.Fixtures.Core.DTO.MediaContributors;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.UpdateBook;

/// <summary>
/// Contains unit tests for the <see cref="UpdateBookCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateBookCommandValidatorTests
{
    private readonly UpdateBookCommandFixture _commandBookFixture = new();
    private readonly UpdateBookCommandValidator _validator = new();
    private readonly WrittenContentMetadataDtoFixture _writtenContentMetadataDtoFixture = new();
    private readonly ReleaseInfoDtoFixture _releaseInfoDtoFixture = new();
    private readonly LanguageInfoDtoFixture _languageInfoDtoFixture = new();
    private readonly IsbnDtoFixture _isbnDtoFixture = new();
    private readonly MediaContributorDtoFixture _mediaContributorDtoFixture = new();
    private readonly BookRatingDtoFixture _bookRatingDtoFixture = new();

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveValidationErrors()
    {
        // Arrange
        UpdateBookCommand bookCommand = _commandBookFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_WhenIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        UpdateBookCommand bookCommand = _commandBookFixture.Create(id: Guid.Empty);

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.BookIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenMetadataIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateBookCommand bookCommand = _commandBookFixture.Create(includeMetadata: false);

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.MetadataCannotBeNull);
    }

    [Fact]
    public void Validate_WhenTitleIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(includeTitle: false));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.TitleCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTitleExceeds255Characters_ShouldHaveValidationError()
    {
        // Arrange
        UpdateBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(title: new Faker().Random.String2(300)));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.TitleMustBeMaximum255CharactersLong);
    }

    [Fact]
    public void Validate_WhenReleaseInfoIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(includeReleaseInfo: false));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.ReleaseInfoCannotBeNull);
    }

    [Fact]
    public void Validate_WhenReReleaseYearIsBeforeOriginalReleaseYear_ShouldHaveValidationError()
    {
        // Arrange
        UpdateBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseYear: 2001, reReleaseYear: 2000, includeReReleaseDate: false)));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.ReReleaseYearCannotBeEarlierThanOriginalReleaseYear);
    }

    [Fact]
    public void Validate_WhenGenresIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(includeGenres: false));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.GenresListCannotBeNull);
    }

    [Fact]
    public void Validate_WhenTagsIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(includeTags: false));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.TagsListCannotBeNull);
    }

    [Fact]
    public void Validate_WhenLanguageCodeIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        UpdateBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(languageCode: string.Empty)));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageCodeCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenFormatIsInvalid_ShouldHaveValidationError()
    {
        // Arrange
        UpdateBookCommand bookCommand = _commandBookFixture.Create(format: (BookFormat)99);

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.UnknownBookFormat);
    }

    [Fact]
    public void Validate_WhenEditionExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        UpdateBookCommand bookCommand = _commandBookFixture.Create(edition: new Faker().Random.String2(51));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.EditionMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenVolumeNumberIsNegative_ShouldHaveValidationError()
    {
        // Arrange
        UpdateBookCommand bookCommand = _commandBookFixture.Create(volumeNumber: -1);

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.VolumeNumberMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenAsinIsNotTenCharacters_ShouldHaveValidationError()
    {
        // Arrange
        UpdateBookCommand bookCommand = _commandBookFixture.Create(asin: new Faker().Random.String2(9));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.AsinMustBe10CharactersLong);
    }

    [Fact]
    public void Validate_WhenGoodreadsIdIsNonNumeric_ShouldHaveValidationError()
    {
        // Arrange
        UpdateBookCommand bookCommand = _commandBookFixture.Create(goodreadsId: "abc123");

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.GoodreadsIdMustBeNumeric);
    }

    [Fact]
    public void Validate_WhenLccnHasInvalidFormat_ShouldHaveValidationError()
    {
        // Arrange
        UpdateBookCommand bookCommand = _commandBookFixture.Create(lccn: "invalid123");

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.InvalidLccnFormat);
    }

    [Fact]
    public void Validate_WhenOclcNumberHasInvalidFormat_ShouldHaveValidationError()
    {
        // Arrange
        UpdateBookCommand bookCommand = _commandBookFixture.Create(oclcNumber: "invalid_oclc_number");

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.InvalidOclcFormat);
    }

    [Fact]
    public void Validate_WhenOpenLibraryIdHasInvalidFormat_ShouldHaveValidationError()
    {
        // Arrange
        UpdateBookCommand bookCommand = _commandBookFixture.Create(openLibraryId: "InvalidID");

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.InvalidOpenLibraryId);
    }

    [Fact]
    public void Validate_WhenGoogleBooksIdHasInvalidFormat_ShouldHaveValidationError()
    {
        // Arrange
        UpdateBookCommand bookCommand = _commandBookFixture.Create(googleBooksId: "invalid!");

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.GoogleBooksIdMustBe12CharactersLong);
    }

    [Fact]
    public void Validate_WhenBarnesAndNobleIdIsNotTenDigits_ShouldHaveValidationError()
    {
        // Arrange
        UpdateBookCommand bookCommand = _commandBookFixture.Create(barnesAndNobleId: "abc123");

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.BarnesAndNoblesIdMustBe10CharactersLong);
    }

    [Fact]
    public void Validate_WhenAppleBooksIdHasInvalidFormat_ShouldHaveValidationError()
    {
        // Arrange
        UpdateBookCommand bookCommand = _commandBookFixture.Create(appleBooksId: "395211");

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.InvalidAppleBooksIdFormat);
    }

    [Fact]
    public void Validate_WhenIsbnsIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateBookCommand bookCommand = _commandBookFixture.Create(includeOptionalProperties: false);

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.IsbnListCannotBeNull);
    }

    [Fact]
    public void Validate_WhenIsbnValueIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        UpdateBookCommand bookCommand = _commandBookFixture.Create(isbns: [_isbnDtoFixture.Create(value: string.Empty)]);

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.IsbnValueCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenIsbn13ValueIsInvalid_ShouldHaveValidationError()
    {
        // Arrange
        UpdateBookCommand bookCommand = _commandBookFixture.Create(isbns: [_isbnDtoFixture.Create(value: "invalid", format: IsbnFormat.Isbn13)]);

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.InvalidIsbn13Format);
    }

    [Fact]
    public void Validate_WhenContributorsIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateBookCommand bookCommand = _commandBookFixture.Create(includeOptionalProperties: false);

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaContributor.ContributorsListCannotBeNull);
    }

    [Fact]
    public void Validate_WhenContributorDisplayNameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        UpdateBookCommand bookCommand = _commandBookFixture.Create(contributors: [_mediaContributorDtoFixture.Create(displayName: string.Empty)]);

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaContributor.ContributorDisplayNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenRatingsIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateBookCommand bookCommand = _commandBookFixture.Create(includeOptionalProperties: false);

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.RatingsListCannotBeNull);
    }

    [Fact]
    public void Validate_WhenRatingValueIsNegative_ShouldHaveValidationError()
    {
        // Arrange
        UpdateBookCommand bookCommand = _commandBookFixture.Create(ratings: [_bookRatingDtoFixture.Create(value: -1)]);

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.RatingValueMustBePositive);
    }
}
