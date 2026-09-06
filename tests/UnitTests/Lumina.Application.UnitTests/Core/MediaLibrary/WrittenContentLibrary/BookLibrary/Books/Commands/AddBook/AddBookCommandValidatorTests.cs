#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.AddBook;
using Lumina.Application.Fixtures.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.AddBook;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.Fixtures.Core.DTO.Common;
using Lumina.Contracts.Fixtures.Core.DTO.MediaContributors;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.AddBook;

/// <summary>
/// Contains unit tests for the <see cref="AddBookCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddBookCommandValidatorTests
{
    private readonly AddBookCommandFixture _commandBookFixture = new();
    private readonly AddBookCommandValidator _validator = new();
    private readonly WrittenContentMetadataDtoFixture _writtenContentMetadataDtoFixture = new();
    private readonly ReleaseInfoDtoFixture _releaseInfoDtoFixture = new();
    private readonly GenreDtoFixture _genreDtoFixture = new();
    private readonly TagDtoFixture _tagDtoFixture = new();
    private readonly LanguageInfoDtoFixture _languageInfoDtoFixture = new();
    private readonly IsbnDtoFixture _isbnDtoFixture = new();
    private readonly BookRatingDtoFixture _bookRatingDtoFixture = new();
    private readonly MediaContributorDtoFixture _mediaContributorDtoFixture = new();

    [Fact]
    public void Validate_WhenTitleIsNull_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(includeTitle: false));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.TitleCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTitleExceeds255Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(title: new Faker().Random.String2(300)));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.TitleMustBeMaximum255CharactersLong);
    }

    [Fact]
    public void Validate_WhenTitleIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(title: new Faker().Random.String2(200)));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.TitleCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenOriginalTitleExceeds255Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(originalTitle: new Faker().Random.String2(300)));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.OriginalTitleMustBeMaximum255CharactersLong);
    }

    [Fact]
    public void Validate_WhenOriginalTitleIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(originalTitle: new Faker().Random.String2(200)));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.OriginalTitleMustBeMaximum255CharactersLong);
    }

    [Fact]
    public void Validate_WhenOriginalTitleIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(includeOriginalTitle: false));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.OriginalTitleMustBeMaximum255CharactersLong);
    }

    [Fact]
    public void Validate_WhenDescriptionExceeds2000Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(description: new Faker().Random.String2(2001)));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.DescriptionMustBeMaximum2000CharactersLong);
    }

    [Fact]
    public void Validate_WhenDescriptionIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(description: new Faker().Random.String2(1500)));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.DescriptionMustBeMaximum2000CharactersLong);
    }

    [Fact]
    public void Validate_WhenDescriptionIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(includeDescription: false));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.DescriptionMustBeMaximum2000CharactersLong);
    }

    [Fact]
    public void Validate_WhenDescriptionIsEmpty_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(description: string.Empty));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.DescriptionMustBeMaximum2000CharactersLong);
    }

    [Fact]
    public void Validate_WhenReleaseInfoIsNull_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(includeReleaseInfo: false));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.ReleaseInfoCannotBeNull);
    }

    [Fact]
    public void Validate_WhenOriginalReleaseYearIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(includeOriginalReleaseDate: false, originalReleaseYear: new Faker().Random.Int(2000, 2005), reReleaseYear: new Faker().Random.Int(2005, 2010), includeReReleaseDate: false)));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.OriginalReleaseYearMustBeBetween1And9999);
    }

    [Fact]
    public void Validate_WhenOriginalReleaseYearIsLessThan1_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(releaseInfo: new ReleaseInfoDto(null, 0, null, null, null, null)));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.OriginalReleaseYearMustBeBetween1And9999);
    }

    [Fact]
    public void Validate_WhenOriginalReleaseYearIsGreaterThan9999_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(releaseInfo: new ReleaseInfoDto(null, 10000, null, null, null, null)));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.OriginalReleaseYearMustBeBetween1And9999);
    }

    [Fact]
    public void Validate_WhenReReleaseYearIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseYear: new Faker().Random.Int(2000, 2005), reReleaseYear: new Faker().Random.Int(2005, 2010), includeReReleaseDate: false)));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.ReReleaseYearMustBeBetween1And9999);
    }

    [Fact]
    public void Validate_WhenReReleaseYearIsLessThan1_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(releaseInfo: new ReleaseInfoDto(null, null, null, 0, null, null)));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.ReReleaseYearMustBeBetween1And9999);
    }

    [Fact]
    public void Validate_WhenReReleaseYearIsGreaterThan9999_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(releaseInfo: new ReleaseInfoDto(null, null, null, 10000, null, null)));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.ReReleaseYearMustBeBetween1And9999);
    }

    [Fact]
    public void Validate_WhenReleaseCountryIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(releaseCountry: new Faker().Random.String2(2).ToUpper())));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.CountryCodeMustBe2CharactersLong);
    }

    [Fact]
    public void Validate_WhenReleaseCountryIsInvalid_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(releaseCountry: new Faker().Random.String2(3))));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.CountryCodeMustBe2CharactersLong);
    }

    [Fact]
    public void Validate_WhenReleaseVersionIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(releaseVersion: new Faker().Random.String2(50))));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.ReleaseVersionMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenReleaseVersionExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(releaseVersion: new Faker().Random.String2(51))));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.ReleaseVersionMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenReReleaseYearIsAfterOriginalReleaseYear_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseYear: 2000, reReleaseYear: 2001, includeReReleaseDate: false)));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.ReReleaseYearCannotBeEarlierThanOriginalReleaseYear);
    }

    [Fact]
    public void Validate_WhenReReleaseYearIsBeforeOriginalReleaseYear_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseYear: 2001, reReleaseYear: 2000, includeReReleaseDate: false)));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.ReReleaseYearCannotBeEarlierThanOriginalReleaseYear);
    }

    [Fact]
    public void Validate_WhenReReleaseDateIsAfterOriginalReleaseDate_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseDate: new DateOnly(2000, 1, 1), reReleaseDate: new DateOnly(2001, 1, 1))));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.ReReleaseDateCannotBeEarlierThanOriginalReleaseDate);
    }

    [Fact]
    public void Validate_WhenReReleaseDateIsBeforeOriginalReleaseDate_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseDate: new DateOnly(2001, 1, 1), reReleaseDate: new DateOnly(2000, 1, 1))));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.ReReleaseDateCannotBeEarlierThanOriginalReleaseDate);
    }

    [Fact]
    public void Validate_WhenGenresIsNull_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(includeGenres: false));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.GenresListCannotBeNull);
    }

    [Fact]
    public void Validate_WhenGenreNameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(genres: [_genreDtoFixture.Create(name: string.Empty)]));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.GenreNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenGenreNameExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(genres: [_genreDtoFixture.Create(name: new Faker().Random.String2(51))]));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.GenreNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenGenresAreValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(genres: [_genreDtoFixture.Create(name: new Faker().Random.String2(50))]));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.GenresListCannotBeNull);
    }

    [Fact]
    public void Validate_WhenTagsIsNull_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(includeTags: false));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.TagsListCannotBeNull);
    }

    [Fact]
    public void Validate_WhenTagNameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(tags: [_tagDtoFixture.Create(name: string.Empty)]));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.TagNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTagNameExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(tags: [_tagDtoFixture.Create(name: new Faker().Random.String2(51))]));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.TagNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenTagsAreValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(tags: [_tagDtoFixture.Create(name: new Faker().Random.String2(50))]));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.TagsListCannotBeNull);
    }

    [Fact]
    public void Validate_WhenLanguageIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(includeLanguage: false));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageCodeCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenLanguageCodeIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(languageCode: string.Empty)));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageCodeCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenLanguageCodeExceeds2Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(languageCode: new Faker().Random.String2(3))));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageCodeMustBe2CharactersLong);
    }

    [Fact]
    public void Validate_WhenLanguageNameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(languageName: string.Empty)));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenLanguageNameExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(languageName: new Faker().Random.String2(51))));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenLanguageNativeNameIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(includeNativeName: false)));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenLanguageNativeNameExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(nativeName: new Faker().Random.String2(51))));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenOriginalLanguageIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(includeOriginalLanguage: false));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageCodeCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenOriginalLanguageCodeIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(languageCode: string.Empty)));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageCodeCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenOriginalLanguageCodeExceeds2Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(languageCode: new Faker().Random.String2(3))));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageCodeMustBe2CharactersLong);
    }

    [Fact]
    public void Validate_WhenOriginalLanguageNameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(languageName: string.Empty)));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenOriginalLanguageNameExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(languageName: new Faker().Random.String2(51))));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenOriginalLanguageNativeNameIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(includeNativeName: false)));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenOriginalLanguageNativeNameExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(nativeName: new Faker().Random.String2(51))));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenCalledWithEmptyPublisher_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(includePublisher: false));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.PublisherMustBeMaximum100CharactersLong);
    }

    [Fact]
    public void Validate_WhenCalledWithValidPublisher_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(publisher: new Faker().Random.String2(100)));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.PublisherMustBeMaximum100CharactersLong);
    }

    [Fact]
    public void Validate_WhenCalledWithInvalidLengthPublisher_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(publisher: new Faker().Random.String2(101)));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.PublisherMustBeMaximum100CharactersLong);
    }

    [Fact]
    public void Validate_WhenPageCountIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(includePageCount: false));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.PageCountMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenPageCountIsZero_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(pageCount: 0));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.PageCountMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenPageCountIsNegative_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(pageCount: -1));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.PageCountMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenPageCountIsPositive_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(pageCount: 100));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.PageCountMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenFormatIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(includeOptionalProperties: false);

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.UnknownBookFormat);
    }

    [Fact]
    public void Validate_WhenFormatIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(format: BookFormat.Hardcover);

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.UnknownBookFormat);
    }

    [Fact]
    public void Validate_WhenFormatIsInvalid_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(format: (BookFormat)99);

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.UnknownBookFormat);
    }

    [Fact]
    public void Validate_WhenEditionIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(includeOptionalProperties: false);

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.EditionMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenEditionIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(edition: new Faker().Random.String2(50));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.EditionMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenEditionExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(edition: new Faker().Random.String2(51));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.EditionMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenVolumeNumberIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(includeOptionalProperties: false);

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.VolumeNumberMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenVolumeNumberIsZero_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(volumeNumber: 0);

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.VolumeNumberMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenVolumeNumberIsNegative_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(volumeNumber: -1);

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.VolumeNumberMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenVolumeNumberIsPositive_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(volumeNumber: 1);

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.VolumeNumberMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenSeriesIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.TitleCannotBeEmpty);
    }

    //[Fact]
    //public void Validate_WhenSeriesTitleIsEmpty_ShouldHaveValidationError()
    //{
    //    // Arrange
    //    var bookCommand = _commandBookFixture.Create();
    //    bookCommand = bookCommand with { Series = bookCommand.Series! with { Title = string.Empty } };

    //    // Act
    //    var result = _validator.TestValidate(bookCommand);

    //    // Assert
    //    result.ShouldHaveValidationErrorFor(x => x.Series.Title).WithErrorMessage(Errors.Metadata.TitleCannotBeEmpty.Description);
    //}

    //[Fact]
    //public void Validate_WhenSeriesTitleExceeds255Characters_ShouldHaveValidationError()
    //{
    //    // Arrange
    //    var bookCommand = _commandBookFixture.Create();
    //    bookCommand = bookCommand with { Series = bookCommand.Series! with { Title = new Faker().Random.String2(256) } };

    //    // Act
    //    var result = _validator.TestValidate(bookCommand);

    //    // Assert
    //    result.ShouldHaveValidationErrorFor(x => x.Series.Title).WithErrorMessage(Errors.Metadata.TitleMustBeMaximum255CharactersLong.Description);
    //}

    //[Fact]
    //public void Validate_WhenSeriesTitleIsValid_ShouldNotHaveValidationError()
    //{
    //    // Arrange
    //    var bookCommand = _commandBookFixture.Create();
    //    bookCommand = bookCommand with { Series = bookCommand.Series! with { Title = new Faker().Random.String2(200) } };

    //    // Act
    //    var result = _validator.TestValidate(bookCommand);

    //    // Assert
    //    result.ShouldNotHaveValidationErrorFor(x => x.Series.Title);
    //}

    [Fact]
    public void Validate_WhenAsinIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(asin: new Faker().Random.String2(10));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.AsinMustBe10CharactersLong);
    }

    [Fact]
    public void Validate_WhenAsinIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(includeOptionalProperties: false);

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.AsinMustBe10CharactersLong);
    }

    [Fact]
    public void Validate_WhenAsinIsNotTenCharacters_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(asin: new Faker().Random.String2(9));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.AsinMustBe10CharactersLong);
    }

    [Fact]
    public void Validate_WhenGoodreadsIdIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(includeOptionalProperties: false);

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.GoodreadsIdMustBeNumeric);
    }

    [Fact]
    public void Validate_WhenGoodreadsIdIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(goodreadsId: "123456789");

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.GoodreadsIdMustBeNumeric);
    }

    [Fact]
    public void Validate_WhenGoodreadsIdIsNonNumeric_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(goodreadsId: "abc123");

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.GoodreadsIdMustBeNumeric);
    }

    [Fact]
    public void Validate_WhenGoodreadsIdContainsSpaces_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(goodreadsId: "123 456");

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.GoodreadsIdMustBeNumeric);
    }

    [Fact]
    public void Validate_WhenGoodreadsIdContainsSpecialCharacters_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(goodreadsId: "123-456");

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.GoodreadsIdMustBeNumeric);
    }

    [Fact]
    public void Validate_WhenLccnIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(includeOptionalProperties: false);

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.InvalidLccnFormat);
    }

    [Fact]
    public void Validate_WhenLccnIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(lccn: "n78890351");

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.InvalidLccnFormat);
    }

    [Fact]
    public void Validate_WhenLccnHasInvalidFormat_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(lccn: "invalid123");

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.InvalidLccnFormat);
    }

    [Fact]
    public void Validate_WhenLccnIsTooLong_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(lccn: new Faker().Random.String2(15));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.InvalidLccnFormat);
    }

    [Fact]
    public void Validate_WhenLccnIsTooShort_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(lccn: "n12");

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.InvalidLccnFormat);
    }

    [Fact]
    public void Validate_WhenCalledWithEmptyOclcNumber_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(includeOptionalProperties: false);

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.InvalidOclcFormat);
    }

    [Fact]
    public void Validate_WhenCalledWithValidOclcNumberFormat1_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(oclcNumber: "ocm12345678");

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.InvalidOclcFormat);
    }

    [Fact]
    public void Validate_WhenCalledWithValidOclcNumberFormat2_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(oclcNumber: "ocn123456789");

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.InvalidOclcFormat);
    }

    [Fact]
    public void Validate_WhenCalledWithValidOclcNumberFormat3_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(oclcNumber: "on1234567890");

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.InvalidOclcFormat);
    }

    [Fact]
    public void Validate_WhenCalledWithValidOclcNumberFormat4_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(oclcNumber: "(OCoLC)1234567890");

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.InvalidOclcFormat);
    }

    [Fact]
    public void Validate_WhenCalledWithValidOclcNumberFormat5_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(oclcNumber: "12345678");

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.InvalidOclcFormat);
    }

    [Fact]
    public void Validate_WhenCalledWithInvalidOclcNumber_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(oclcNumber: "invalid_oclc_number");

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.InvalidOclcFormat);
    }

    [Fact]
    public void Validate_WhenOpenLibraryIdIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(includeOptionalProperties: false);

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.InvalidOpenLibraryId);
    }

    [Fact]
    public void Validate_WhenOpenLibraryIdIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(openLibraryId: "OL123456M");

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.InvalidOpenLibraryId);
    }

    [Fact]
    public void Validate_WhenOpenLibraryIdIsInvalid_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(openLibraryId: "InvalidID");

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.InvalidOpenLibraryId);
    }

    [Fact]
    public void Validate_WhenOpenLibraryIdStartsWithOLButIsInvalidFormat_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(openLibraryId: "OL123ABC");

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.InvalidOpenLibraryId);
    }

    [Fact]
    public void Validate_WhenOpenLibraryIdHasValidFormatButInvalidSuffix_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(openLibraryId: "OL123456X");

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.InvalidOpenLibraryId);
    }

    [Fact]
    public void Validate_WhenCalledWithEmptyLibraryThingId_ShouldAddBook()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(includeOptionalProperties: false);

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.LibraryThingIdMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenCalledWithValidLibraryThingId_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(libraryThingId: new Faker().Random.String2(50));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.LibraryThingIdMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenCalledWithInvalidLengthLibraryThingId_ShouldReturnBadRequest()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(libraryThingId: new Faker().Random.String2(51));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.LibraryThingIdMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenCalledWithEmptyGoogleBooksId_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(includeOptionalProperties: false);

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.GoogleBooksIdMustBe12CharactersLong);
    }

    [Fact]
    public void Validate_WhenCalledWithInvalidLengthGoogleBooksId_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(googleBooksId: new Faker().Random.String2(11));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.GoogleBooksIdMustBe12CharactersLong);
    }

    [Fact]
    public void Validate_WhenCalledWithInvalidFormatGoogleBooksId_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(googleBooksId: new Faker().Random.String2(11) + " ");

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.InvalidGoogleBooksIdFormat);
    }

    [Fact]
    public void Validate_WhenCalledWithValidGoogleBooksId_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(googleBooksId: new Faker().Random.String2(12, "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_-"));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.GoogleBooksIdMustBe12CharactersLong);
    }

    [Fact]
    public void Validate_WhenCalledWithEmptyBarnesAndNobleId_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(includeOptionalProperties: false);

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.BarnesAndNoblesIdMustBe10CharactersLong);
    }

    [Fact]
    public void Validate_WhenCalledWithInvalidLengthBarnesAndNobleId_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(barnesAndNobleId: new Faker().Random.String2(11));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.BarnesAndNoblesIdMustBe10CharactersLong);
    }

    [Fact]
    public void Validate_WhenCalledWithNonNumericBarnesAndNobleId_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(barnesAndNobleId: new Faker().Random.AlphaNumeric(10));

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.InvalidBarnesAndNoblesIdFormat);
    }

    [Fact]
    public void Validate_WhenCalledWithValidBarnesAndNobleId_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(barnesAndNobleId: new Faker().Random.Number(1000000000, 999999999).ToString());

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.BarnesAndNoblesIdMustBe10CharactersLong);
    }

    [Fact]
    public void AddBook_WhenCalledWithEmptyAppleBooksId_ShouldAddBook()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(includeOptionalProperties: false);

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.InvalidAppleBooksIdFormat);
    }

    [Fact]
    public void AddBook_WhenCalledWithValidAppleBooksId_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(appleBooksId: "id123456");

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.InvalidAppleBooksIdFormat);
    }

    [Fact]
    public void AddBook_WhenCalledWithInvalidAppleBooksId_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(appleBooksId: "invalid_id");

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.InvalidAppleBooksIdFormat);
    }

    [Fact]
    public void Validate_WhenCalledWithNullIsbns_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(includeOptionalProperties: false);

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.IsbnListCannotBeNull);
    }

    [Fact]
    public void Validate_WhenCalledWithEmptyIsbnValue_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(isbns: [_isbnDtoFixture.Create(value: string.Empty)]);

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.IsbnValueCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenCalledWithInvalidIsbn10Value_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(isbns: [_isbnDtoFixture.Create(value: new Faker().Random.String2(5), format: IsbnFormat.Isbn10)]);

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.InvalidIsbn10Format);
    }

    [Fact]
    public void Validate_WhenCalledWithInvalidIsbn13Value_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(isbns: [_isbnDtoFixture.Create(value: new Faker().Random.String2(5), format: IsbnFormat.Isbn13)]);

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.InvalidIsbn13Format);
    }

    [Fact]
    public void Validate_WhenCalledWithInvalidIsbnFormat_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(isbns: [_isbnDtoFixture.Create(format: (IsbnFormat)99)]);

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.UnknownIsbnFormat);
    }

    [Fact]
    public void Validate_WhenCalledWithValidIsbn10_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(isbns: [_isbnDtoFixture.Create(value: "0-306-40615-2", format: IsbnFormat.Isbn10)]);

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.InvalidIsbn10Format);
    }

    [Fact]
    public void Validate_WhenCalledWithValidIsbn13_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(isbns: [_isbnDtoFixture.Create(value: "978-3-16-148410-0", format: IsbnFormat.Isbn13)]);

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.InvalidIsbn13Format);
    }

    [Fact]
    public void Validate_WhenCalledWithNullContributors_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(includeOptionalProperties: false);

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaContributor.ContributorsListCannotBeNull);
    }

    [Fact]
    public void Validate_WhenCalledWithNullContributorName_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(contributors: [new MediaContributorDto(null, new MediaContributorRoleDto("author", MediaContributorRoleCategory.Author))]);

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaContributor.ContributorNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenCalledWithInvalidLengthContributorDisplayName_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(contributors: [_mediaContributorDtoFixture.Create(displayName: new Faker().Random.String2(101))]);

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaContributor.ContributorDisplayNameMustBeMaximum100CharactersLong);
    }

    [Fact]
    public void Validate_WhenCalledWithEmptyContributorDisplayName_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(contributors: [_mediaContributorDtoFixture.Create(displayName: string.Empty)]);

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaContributor.ContributorDisplayNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenCalledWithInvalidLengthContributorLegalName_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(contributors: [new MediaContributorDto(new MediaContributorNameDto(new Faker().Random.String2(50), new Faker().Random.String2(101)), new MediaContributorRoleDto("author", MediaContributorRoleCategory.Author))]);

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaContributor.ContributorLegalNameMustBeMaximum100CharactersLong);
    }

    [Fact]
    public void Validate_WhenCalledWithNullContributorRole_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(contributors: [new MediaContributorDto(new MediaContributorNameDto(new Faker().Random.String2(50), null), null)]);

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaContributor.ContributorRoleCannotBeNull);
    }

    [Fact]
    public void Validate_WhenCalledWithEmptyContributorRoleName_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(contributors: [new MediaContributorDto(new MediaContributorNameDto(new Faker().Random.String2(50), null), new MediaContributorRoleDto(null, MediaContributorRoleCategory.Author))]);

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaContributor.RoleNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenCalledWithInvalidLengthContributorRoleName_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(contributors: [new MediaContributorDto(new MediaContributorNameDto(new Faker().Random.String2(50), null), new MediaContributorRoleDto(new Faker().Random.String2(51), MediaContributorRoleCategory.Author))]);

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaContributor.RoleNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenCalledWithEmptyContributorRoleCategory_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(contributors: [new MediaContributorDto(new MediaContributorNameDto(new Faker().Random.String2(50), null), new MediaContributorRoleDto("author", null))]);

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaContributor.RoleCategoryCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenCalledWithNullRatings_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(includeOptionalProperties: false);

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.RatingsListCannotBeNull);
    }

    [Fact]
    public void Validate_WhenCalledWithNegativeRatingValue_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(ratings: [_bookRatingDtoFixture.Create(value: -1)]);

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.RatingValueMustBePositive);
    }

    [Fact]
    public void Validate_WhenCalledWithRatingValueGreaterThanMaxValue_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(ratings: [_bookRatingDtoFixture.Create(value: 6, maxValue: 5)]);

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.RatingValueCannotBeGreaterThanMaxValue);
    }

    [Fact]
    public void Validate_WhenCalledWithNegativeMaxRatingValue_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(ratings: [_bookRatingDtoFixture.Create(maxValue: -1)]);

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.RatingMaxValueMustBePositive);
    }

    [Fact]
    public void Validate_WhenCalledWithNegativeVoteCount_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(ratings: [_bookRatingDtoFixture.Create(voteCount: -1)]);

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.RatingVoteCountMustBePositive);
    }

    [Fact]
    public void Validate_WhenCalledWithValidRatings_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(ratings: [_bookRatingDtoFixture.Create(value: 4, maxValue: 5, voteCount: 100)]);

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.RatingsListCannotBeNull);
    }

    [Fact]
    public void Validate_WhenCalledWithNullVoteCount_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(ratings: [_bookRatingDtoFixture.Create(includeOptionalProperties: false)]);

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.RatingVoteCountMustBePositive);
    }

    [Fact]
    public void Validate_WhenLibraryIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(libraryId: Guid.Empty);

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.BookLibraryCannotBeNull);
    }
}
