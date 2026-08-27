#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BooksLibrary.Books.Commands.AddBook;
using Lumina.Application.Fixtures.Core.MediaLibrary.WrittenContentLibrary.BooksLibrary.Books.Commands.AddBook;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.WrittenContentLibrary.BooksLibrary.Books.Commands.AddBook;

/// <summary>
/// Contains unit tests for the <see cref="AddBookCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddBookCommandValidatorTests
{
    private readonly AddBookCommandFixture _commandBookFixture = new();
    private readonly AddBookCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenTitleIsNull_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { Title = null! } };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.TitleCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTitleExceeds255Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { Title = new Faker().Random.String2(300) } };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.TitleMustBeMaximum255CharactersLong);
    }

    [Fact]
    public void Validate_WhenTitleIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { Title = new Faker().Random.String2(200) } };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.TitleCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenOriginalTitleExceeds255Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { OriginalTitle = new Faker().Random.String2(300) } };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.OriginalTitleMustBeMaximum255CharactersLong);
    }

    [Fact]
    public void Validate_WhenOriginalTitleIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { OriginalTitle = new Faker().Random.String2(200) } };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.OriginalTitleMustBeMaximum255CharactersLong);
    }

    [Fact]
    public void Validate_WhenOriginalTitleIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { OriginalTitle = null! } };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.OriginalTitleMustBeMaximum255CharactersLong);
    }

    [Fact]
    public void Validate_WhenDescriptionExceeds2000Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { Description = new Faker().Random.String2(2001) } };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.DescriptionMustBeMaximum2000CharactersLong);
    }

    [Fact]
    public void Validate_WhenDescriptionIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { Description = new Faker().Random.String2(1500) } };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.DescriptionMustBeMaximum2000CharactersLong);
    }

    [Fact]
    public void Validate_WhenDescriptionIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { Description = null! } };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.DescriptionMustBeMaximum2000CharactersLong);
    }

    [Fact]
    public void Validate_WhenDescriptionIsEmpty_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { Description = string.Empty } };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.DescriptionMustBeMaximum2000CharactersLong);
    }

    [Fact]
    public void Validate_WhenReleaseInfoIsNull_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { ReleaseInfo = null! } };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.ReleaseInfoCannotBeNull);
    }

    [Fact]
    public void Validate_WhenOriginalReleaseYearIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { ReleaseInfo = bookCommand.Metadata.ReleaseInfo! with { OriginalReleaseDate = null!, OriginalReleaseYear = new Faker().Random.Int(2000, 2005), ReReleaseYear = new Faker().Random.Int(2005, 2010), ReReleaseDate = null! } } };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.OriginalReleaseYearMustBeBetween1And9999);
    }

    [Fact]
    public void Validate_WhenOriginalReleaseYearIsLessThan1_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { ReleaseInfo = bookCommand.Metadata.ReleaseInfo! with { OriginalReleaseYear = 0 } } };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.OriginalReleaseYearMustBeBetween1And9999);
    }

    [Fact]
    public void Validate_WhenOriginalReleaseYearIsGreaterThan9999_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { ReleaseInfo = bookCommand.Metadata.ReleaseInfo! with { OriginalReleaseYear = 10000 } } };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.OriginalReleaseYearMustBeBetween1And9999);
    }

    [Fact]
    public void Validate_WhenReReleaseYearIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with
        {
            Metadata = bookCommand.Metadata! with
            {
                ReleaseInfo = bookCommand.Metadata.ReleaseInfo!
            with
                { OriginalReleaseYear = new Faker().Random.Int(2000, 2005), ReReleaseYear = new Faker().Random.Int(2005, 2010), ReReleaseDate = null! }
            }
        };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.ReReleaseYearMustBeBetween1And9999);
    }

    [Fact]
    public void Validate_WhenReReleaseYearIsLessThan1_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { ReleaseInfo = bookCommand.Metadata.ReleaseInfo! with { ReReleaseYear = 0 } } };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.ReReleaseYearMustBeBetween1And9999);
    }

    [Fact]
    public void Validate_WhenReReleaseYearIsGreaterThan9999_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { ReleaseInfo = bookCommand.Metadata.ReleaseInfo! with { ReReleaseYear = 10000 } } };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.ReReleaseYearMustBeBetween1And9999);
    }

    [Fact]
    public void Validate_WhenReleaseCountryIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { ReleaseInfo = bookCommand.Metadata.ReleaseInfo! with { ReleaseCountry = new Faker().Random.String2(2).ToUpper() } } };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.CountryCodeMustBe2CharactersLong);
    }

    [Fact]
    public void Validate_WhenReleaseCountryIsInvalid_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { ReleaseInfo = bookCommand.Metadata.ReleaseInfo! with { ReleaseCountry = new Faker().Random.String2(3) } } };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.CountryCodeMustBe2CharactersLong);
    }

    [Fact]
    public void Validate_WhenReleaseVersionIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { ReleaseInfo = bookCommand.Metadata.ReleaseInfo! with { ReleaseVersion = new Faker().Random.String2(50) } } };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.ReleaseVersionMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenReleaseVersionExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { ReleaseInfo = bookCommand.Metadata.ReleaseInfo! with { ReleaseVersion = new Faker().Random.String2(51) } } };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.ReleaseVersionMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenReReleaseYearIsAfterOriginalReleaseYear_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { ReleaseInfo = bookCommand.Metadata.ReleaseInfo! with { OriginalReleaseYear = 2000, ReReleaseYear = 2001, ReReleaseDate = null! } } };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.ReReleaseYearCannotBeEarlierThanOriginalReleaseYear);
    }

    [Fact]
    public void Validate_WhenReReleaseYearIsBeforeOriginalReleaseYear_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { ReleaseInfo = bookCommand.Metadata.ReleaseInfo! with { OriginalReleaseYear = 2001, ReReleaseYear = 2000, ReReleaseDate = null! } } };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.ReReleaseYearCannotBeEarlierThanOriginalReleaseYear);
    }

    [Fact]
    public void Validate_WhenReReleaseDateIsAfterOriginalReleaseDate_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { ReleaseInfo = bookCommand.Metadata.ReleaseInfo! with { OriginalReleaseDate = new DateOnly(2000, 1, 1), ReReleaseDate = new DateOnly(2001, 1, 1) } } };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.ReReleaseDateCannotBeEarlierThanOriginalReleaseDate);
    }

    [Fact]
    public void Validate_WhenReReleaseDateIsBeforeOriginalReleaseDate_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { ReleaseInfo = bookCommand.Metadata.ReleaseInfo! with { OriginalReleaseDate = new DateOnly(2001, 1, 1), ReReleaseDate = new DateOnly(2000, 1, 1) } } };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.ReReleaseDateCannotBeEarlierThanOriginalReleaseDate);
    }

    [Fact]
    public void Validate_WhenGenresIsNull_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { Genres = null! } };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.GenresListCannotBeNull);
    }

    [Fact]
    public void Validate_WhenGenreNameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { Genres = [.. bookCommand.Metadata.Genres!.Select((genre, index) => index == 0 ? genre with { Name = string.Empty } : genre)] } };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.GenreNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenGenreNameExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { Genres = [.. bookCommand.Metadata.Genres!.Select((genre, index) => index == 0 ? genre with { Name = new Faker().Random.String2(51) } : genre)] } };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.GenreNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenGenresAreValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with
        {
            Metadata = bookCommand.Metadata! with
            {
                Genres = [.. bookCommand.Metadata.Genres!.Select((genre, index) => index == 0 ? genre with { Name = new Faker().Random.String2(50) } : genre)]
            }
        };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.GenresListCannotBeNull);
    }

    [Fact]
    public void Validate_WhenTagsIsNull_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { Tags = null! } };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.TagsListCannotBeNull);
    }

    [Fact]
    public void Validate_WhenTagNameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { Tags = [.. bookCommand.Metadata.Tags!.Select((tag, index) => index == 0 ? tag with { Name = string.Empty } : tag)] } };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.TagNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTagNameExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { Tags = [.. bookCommand.Metadata.Tags!.Select((tag, index) => index == 0 ? tag with { Name = new Faker().Random.String2(51) } : tag)] } };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.TagNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenTagsAreValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with
        {
            Metadata = bookCommand.Metadata! with
            {
                Tags = [.. bookCommand.Metadata.Tags!.Select((tag, index) => index == 0 ? tag with { Name = new Faker().Random.String2(50) } : tag)]
            }
        };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.TagsListCannotBeNull);
    }

    [Fact]
    public void Validate_WhenLanguageIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { Language = null! } };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageCodeCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenLanguageCodeIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { Language = bookCommand.Metadata.Language! with { LanguageCode = string.Empty } } };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageCodeCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenLanguageCodeExceeds2Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { Language = bookCommand.Metadata.Language! with { LanguageCode = new Faker().Random.String2(3) } } };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageCodeMustBe2CharactersLong);
    }

    [Fact]
    public void Validate_WhenLanguageNameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { Language = bookCommand.Metadata.Language! with { LanguageName = string.Empty } } };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenLanguageNameExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { Language = bookCommand.Metadata.Language! with { LanguageName = new Faker().Random.String2(51) } } };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenLanguageNativeNameIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { Language = bookCommand.Metadata.Language! with { NativeName = null! } } };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenLanguageNativeNameExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { Language = bookCommand.Metadata.Language! with { NativeName = new Faker().Random.String2(51) } } };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenOriginalLanguageIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { OriginalLanguage = null! } };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageCodeCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenOriginalLanguageCodeIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { OriginalLanguage = bookCommand.Metadata.OriginalLanguage! with { LanguageCode = string.Empty } } };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageCodeCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenOriginalLanguageCodeExceeds2Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { OriginalLanguage = bookCommand.Metadata.OriginalLanguage! with { LanguageCode = new Faker().Random.String2(3) } } };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageCodeMustBe2CharactersLong);
    }

    [Fact]
    public void Validate_WhenOriginalLanguageNameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { OriginalLanguage = bookCommand.Metadata.OriginalLanguage! with { LanguageName = string.Empty } } };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenOriginalLanguageNameExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { OriginalLanguage = bookCommand.Metadata.OriginalLanguage! with { LanguageName = new Faker().Random.String2(51) } } };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenOriginalLanguageNativeNameIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { OriginalLanguage = bookCommand.Metadata.OriginalLanguage! with { NativeName = null! } } };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenOriginalLanguageNativeNameExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { OriginalLanguage = bookCommand.Metadata.OriginalLanguage! with { NativeName = new Faker().Random.String2(51) } } };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenCalledWithEmptyPublisher_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { Publisher = null! } };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.PublisherMustBeMaximum100CharactersLong);
    }

    [Fact]
    public void Validate_WhenCalledWithValidPublisher_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { Publisher = new Faker().Random.String2(100) } };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.PublisherMustBeMaximum100CharactersLong);
    }

    [Fact]
    public void Validate_WhenCalledWithInvalidLengthPublisher_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { Publisher = new Faker().Random.String2(101) } };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.PublisherMustBeMaximum100CharactersLong);
    }

    [Fact]
    public void Validate_WhenPageCountIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { PageCount = null } };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.PageCountMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenPageCountIsZero_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { PageCount = 0 } };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.PageCountMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenPageCountIsNegative_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { PageCount = -1 } };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.PageCountMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenPageCountIsPositive_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { PageCount = 100 } };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.PageCountMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenFormatIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Format = null };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.UnknownBookFormat);
    }

    [Fact]
    public void Validate_WhenFormatIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Format = BookFormat.Hardcover };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.UnknownBookFormat);
    }

    [Fact]
    public void Validate_WhenFormatIsInvalid_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Format = (BookFormat)99 };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.UnknownBookFormat);
    }

    [Fact]
    public void Validate_WhenEditionIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Edition = null };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.EditionMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenEditionIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Edition = new Faker().Random.String2(50) };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.EditionMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenEditionExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Edition = new Faker().Random.String2(51) };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.EditionMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenVolumeNumberIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { VolumeNumber = null };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.VolumeNumberMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenVolumeNumberIsZero_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { VolumeNumber = 0 };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.VolumeNumberMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenVolumeNumberIsNegative_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { VolumeNumber = -1 };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.VolumeNumberMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenVolumeNumberIsPositive_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { VolumeNumber = 1 };

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
        bookCommand = bookCommand with { Series = null };

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
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { ASIN = new Faker().Random.String2(10) };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.AsinMustBe10CharactersLong);
    }

    [Fact]
    public void Validate_WhenAsinIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { ASIN = null };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.AsinMustBe10CharactersLong);
    }

    [Fact]
    public void Validate_WhenAsinIsNotTenCharacters_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { ASIN = new Faker().Random.String2(9) };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.AsinMustBe10CharactersLong);
    }

    [Fact]
    public void Validate_WhenGoodreadsIdIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { GoodreadsId = null };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.GoodreadsIdMustBeNumeric);
    }

    [Fact]
    public void Validate_WhenGoodreadsIdIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { GoodreadsId = "123456789" };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.GoodreadsIdMustBeNumeric);
    }

    [Fact]
    public void Validate_WhenGoodreadsIdIsNonNumeric_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { GoodreadsId = "abc123" };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.GoodreadsIdMustBeNumeric);
    }

    [Fact]
    public void Validate_WhenGoodreadsIdContainsSpaces_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { GoodreadsId = "123 456" };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.GoodreadsIdMustBeNumeric);
    }

    [Fact]
    public void Validate_WhenGoodreadsIdContainsSpecialCharacters_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { GoodreadsId = "123-456" };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.GoodreadsIdMustBeNumeric);
    }

    [Fact]
    public void Validate_WhenLccnIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { LCCN = null };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.InvalidLccnFormat);
    }

    [Fact]
    public void Validate_WhenLccnIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { LCCN = "n78890351" };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.InvalidLccnFormat);
    }

    [Fact]
    public void Validate_WhenLccnHasInvalidFormat_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { LCCN = "invalid123" };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.InvalidLccnFormat);
    }

    [Fact]
    public void Validate_WhenLccnIsTooLong_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { LCCN = new Faker().Random.String2(15) };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.InvalidLccnFormat);
    }

    [Fact]
    public void Validate_WhenLccnIsTooShort_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { LCCN = "n12" };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.InvalidLccnFormat);
    }

    [Fact]
    public void Validate_WhenCalledWithEmptyOclcNumber_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { OCLCNumber = null };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.InvalidOclcFormat);
    }

    [Fact]
    public void Validate_WhenCalledWithValidOclcNumberFormat1_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { OCLCNumber = "ocm12345678" };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.InvalidOclcFormat);
    }

    [Fact]
    public void Validate_WhenCalledWithValidOclcNumberFormat2_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { OCLCNumber = "ocn123456789" };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.InvalidOclcFormat);
    }

    [Fact]
    public void Validate_WhenCalledWithValidOclcNumberFormat3_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { OCLCNumber = "on1234567890" };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.InvalidOclcFormat);
    }

    [Fact]
    public void Validate_WhenCalledWithValidOclcNumberFormat4_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { OCLCNumber = "(OCoLC)1234567890" };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.InvalidOclcFormat);
    }

    [Fact]
    public void Validate_WhenCalledWithValidOclcNumberFormat5_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { OCLCNumber = "12345678" };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.InvalidOclcFormat);
    }

    [Fact]
    public void Validate_WhenCalledWithInvalidOclcNumber_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { OCLCNumber = "invalid_oclc_number" };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.InvalidOclcFormat);
    }

    [Fact]
    public void Validate_WhenOpenLibraryIdIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { OpenLibraryId = null! };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.InvalidOpenLibraryId);
    }

    [Fact]
    public void Validate_WhenOpenLibraryIdIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { OpenLibraryId = "OL123456M" };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.InvalidOpenLibraryId);
    }

    [Fact]
    public void Validate_WhenOpenLibraryIdIsInvalid_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { OpenLibraryId = "InvalidID" };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.InvalidOpenLibraryId);
    }

    [Fact]
    public void Validate_WhenOpenLibraryIdStartsWithOLButIsInvalidFormat_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { OpenLibraryId = "OL123ABC" };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.InvalidOpenLibraryId);
    }

    [Fact]
    public void Validate_WhenOpenLibraryIdHasValidFormatButInvalidSuffix_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { OpenLibraryId = "OL123456X" };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.InvalidOpenLibraryId);
    }

    [Fact]
    public void Validate_WhenCalledWithEmptyLibraryThingId_ShouldAddBook()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { LibraryThingId = null };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.LibraryThingIdMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenCalledWithValidLibraryThingId_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { LibraryThingId = new Faker().Random.String2(50) };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.LibraryThingIdMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenCalledWithInvalidLengthLibraryThingId_ShouldReturnBadRequest()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { LibraryThingId = new Faker().Random.String2(51) };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.LibraryThingIdMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenCalledWithEmptyGoogleBooksId_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { GoogleBooksId = null! };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.GoogleBooksIdMustBe12CharactersLong);
    }

    [Fact]
    public void Validate_WhenCalledWithInvalidLengthGoogleBooksId_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { GoogleBooksId = new Faker().Random.String2(11) };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.GoogleBooksIdMustBe12CharactersLong);
    }

    [Fact]
    public void Validate_WhenCalledWithInvalidFormatGoogleBooksId_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { GoogleBooksId = new Faker().Random.String2(11) + " " };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.InvalidGoogleBooksIdFormat);
    }

    [Fact]
    public void Validate_WhenCalledWithValidGoogleBooksId_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { GoogleBooksId = new Faker().Random.String2(12, "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_-") };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.GoogleBooksIdMustBe12CharactersLong);
    }

    [Fact]
    public void Validate_WhenCalledWithEmptyBarnesAndNobleId_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { BarnesAndNobleId = null! };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.BarnesAndNoblesIdMustBe10CharactersLong);
    }

    [Fact]
    public void Validate_WhenCalledWithInvalidLengthBarnesAndNobleId_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { BarnesAndNobleId = new Faker().Random.String2(11) };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.BarnesAndNoblesIdMustBe10CharactersLong);
    }

    [Fact]
    public void Validate_WhenCalledWithNonNumericBarnesAndNobleId_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { BarnesAndNobleId = new Faker().Random.AlphaNumeric(10) };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.InvalidBarnesAndNoblesIdFormat);
    }

    [Fact]
    public void Validate_WhenCalledWithValidBarnesAndNobleId_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { BarnesAndNobleId = new Faker().Random.Number(1000000000, 999999999).ToString() };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.BarnesAndNoblesIdMustBe10CharactersLong);
    }

    [Fact]
    public void AddBook_WhenCalledWithEmptyAppleBooksId_ShouldAddBook()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { AppleBooksId = null };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.InvalidAppleBooksIdFormat);
    }

    [Fact]
    public void AddBook_WhenCalledWithValidAppleBooksId_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { AppleBooksId = "id123456" };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.InvalidAppleBooksIdFormat);
    }

    [Fact]
    public void AddBook_WhenCalledWithInvalidAppleBooksId_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { AppleBooksId = "invalid_id" };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.InvalidAppleBooksIdFormat);
    }

    [Fact]
    public void Validate_WhenCalledWithNullIsbns_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { ISBNs = null! };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.IsbnListCannotBeNull);
    }

    [Fact]
    public void Validate_WhenCalledWithEmptyIsbnValue_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { ISBNs = [.. bookCommand.ISBNs!.Select((isbn, index) => index == 0 ? isbn with { Value = null! } : isbn)] };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.IsbnValueCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenCalledWithInvalidIsbn10Value_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { ISBNs = [.. bookCommand.ISBNs!.Select((isbn, index) => index == 0 ? isbn with { Value = new Faker().Random.String2(5), Format = IsbnFormat.Isbn10 } : isbn)] };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.InvalidIsbn10Format);
    }

    [Fact]
    public void Validate_WhenCalledWithInvalidIsbn13Value_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { ISBNs = [.. bookCommand.ISBNs!.Select((isbn, index) => index == 0 ? isbn with { Value = new Faker().Random.String2(5), Format = IsbnFormat.Isbn13 } : isbn)] };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.InvalidIsbn13Format);
    }

    [Fact]
    public void Validate_WhenCalledWithInvalidIsbnFormat_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { ISBNs = [.. bookCommand.ISBNs!.Select((isbn, index) => index == 0 ? isbn with { Format = (IsbnFormat)99 } : isbn)] };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.UnknownIsbnFormat);
    }

    [Fact]
    public void Validate_WhenCalledWithValidIsbn10_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { ISBNs = [.. bookCommand.ISBNs!.Select((isbn, index) => index == 0 ? isbn with { Value = "0-306-40615-2", Format = IsbnFormat.Isbn10 } : isbn)] };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.InvalidIsbn10Format);
    }

    [Fact]
    public void Validate_WhenCalledWithValidIsbn13_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { ISBNs = [.. bookCommand.ISBNs!.Select((isbn, index) => index == 0 ? isbn with { Value = "978-3-16-148410-0", Format = IsbnFormat.Isbn13 } : isbn)] };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.InvalidIsbn13Format);
    }

    [Fact]
    public void Validate_WhenCalledWithNullContributors_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Contributors = null! };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaContributor.ContributorsListCannotBeNull);
    }

    [Fact]
    public void Validate_WhenCalledWithNullContributorName_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Contributors = [.. bookCommand.Contributors!.Select((contributor, index) => index == 0 ? contributor with { Name = null! } : contributor)] };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaContributor.ContributorNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenCalledWithInvalidLengthContributorDisplayName_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Contributors = [.. bookCommand.Contributors!.Select((contributor, index) => index == 0 ? contributor with { Name = contributor.Name! with { DisplayName = new Faker().Random.String2(101) } } : contributor)] };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaContributor.ContributorDisplayNameMustBeMaximum100CharactersLong);
    }

    [Fact]
    public void Validate_WhenCalledWithEmptyContributorDisplayName_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Contributors = [.. bookCommand.Contributors!.Select((contributor, index) => index == 0 ? contributor with { Name = contributor.Name! with { DisplayName = null! } } : contributor)] };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaContributor.ContributorDisplayNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenCalledWithInvalidLengthContributorLegalName_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Contributors = [.. bookCommand.Contributors!.Select((contributor, index) => index == 0 ? contributor with { Name = contributor.Name! with { LegalName = new Faker().Random.String2(101) } } : contributor)] };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaContributor.ContributorLegalNameMustBeMaximum100CharactersLong);
    }

    [Fact]
    public void Validate_WhenCalledWithNullContributorRole_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Contributors = [.. bookCommand.Contributors!.Select((contributor, index) => index == 0 ? contributor with { Role = null! } : contributor)] };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaContributor.ContributorRoleCannotBeNull);
    }

    [Fact]
    public void Validate_WhenCalledWithEmptyContributorRoleName_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Contributors = [.. bookCommand.Contributors!.Select((contributor, index) => index == 0 ? contributor with { Role = contributor.Role! with { Name = null! } } : contributor)] };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaContributor.RoleNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenCalledWithInvalidLengthContributorRoleName_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Contributors = [.. bookCommand.Contributors!.Select((contributor, index) => index == 0 ? contributor with { Role = contributor.Role! with { Name = new Faker().Random.String2(51) } } : contributor)] };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaContributor.RoleNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenCalledWithEmptyContributorRoleCategory_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Contributors = [.. bookCommand.Contributors!.Select((contributor, index) => index == 0 ? contributor with { Role = contributor.Role! with { Category = null! } } : contributor)] };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaContributor.RoleCategoryCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenCalledWithNullRatings_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Ratings = null! };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.RatingsListCannotBeNull);
    }

    [Fact]
    public void Validate_WhenCalledWithNegativeRatingValue_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Ratings = [.. bookCommand.Ratings!.Select((rating, index) => index == 0 ? rating with { Value = -1 } : rating)] };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.RatingValueMustBePositive);
    }

    [Fact]
    public void Validate_WhenCalledWithRatingValueGreaterThanMaxValue_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Ratings = [.. bookCommand.Ratings!.Select((rating, index) => index == 0 ? rating with { Value = 6, MaxValue = 5 } : rating)] };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.RatingValueCannotBeGreaterThanMaxValue);
    }

    [Fact]
    public void Validate_WhenCalledWithNegativeMaxRatingValue_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Ratings = [.. bookCommand.Ratings!.Select((rating, index) => index == 0 ? rating with { MaxValue = -1 } : rating)] };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.RatingMaxValueMustBePositive);
    }

    [Fact]
    public void Validate_WhenCalledWithNegativeVoteCount_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Ratings = [.. bookCommand.Ratings!.Select((rating, index) => index == 0 ? rating with { VoteCount = -1 } : rating)] };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.RatingVoteCountMustBePositive);
    }

    [Fact]
    public void Validate_WhenCalledWithValidRatings_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with
        {
            Ratings = [.. bookCommand.Ratings!.Select((rating, index) => index == 0 ? rating
            with
            { Value = 4, MaxValue = 5, VoteCount = 100 } : rating)]
        };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.RatingsListCannotBeNull);
    }

    [Fact]
    public void Validate_WhenCalledWithNullVoteCount_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Ratings = [.. bookCommand.Ratings!.Select((rating, index) => index == 0 ? rating with { VoteCount = null } : rating)] };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.RatingVoteCountMustBePositive);
    }

    [Fact]
    public void Validate_WhenLibraryIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { LibraryId = Guid.Empty };

        // Act
        List<Error> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.BookLibraryCannotBeNull);
    }
}
