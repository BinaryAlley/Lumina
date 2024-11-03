#region ========================================================================= USING =====================================================================================
using Bogus;
using FluentValidation.TestHelper;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BooksLibrary.Books.Commands.AddBook;
using Lumina.Application.UnitTests.Core.WrittenContentLibrary.BooksLibrary.Books.Commands.AddBook.Fixtures;
using Lumina.Domain.Common.Enums.BookLibrary;
using Lumina.Domain.Common.Errors;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Core.WrittenContentLibrary.BooksLibrary.Books.Commands.AddBook;

/// <summary>
/// Contains unit tests for the <see cref="AddBookCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddBookCommandValidatorTests
{
    private readonly AddBookCommandFixture _commandBookFixture;
    private readonly AddBookCommandValidator _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddBookCommandValidatorTests"/> class.
    /// </summary>
    public AddBookCommandValidatorTests()
    {
        _validator = new AddBookCommandValidator();
        _commandBookFixture = new AddBookCommandFixture();
    }

    [Fact]
    public void Validate_WhenTitleIsNull_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { Title = null! } };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Metadata!.Title).WithErrorMessage(Errors.Metadata.TitleCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenTitleExceeds255Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { Title = new Faker().Random.String2(300) } };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Metadata!.Title).WithErrorMessage(Errors.Metadata.TitleMustBeMaximum255CharactersLong.Description);
    }

    [Fact]
    public void Validate_WhenTitleIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { Title = new Faker().Random.String2(200) } };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Metadata!.Title);
    }

    [Fact]
    public void Validate_WhenOriginalTitleExceeds255Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { OriginalTitle = new Faker().Random.String2(300) } };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Metadata!.OriginalTitle).WithErrorMessage(Errors.Metadata.OriginalTitleMustBeMaximum255CharactersLong.Description);
    }

    [Fact]
    public void Validate_WhenOriginalTitleIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { OriginalTitle = new Faker().Random.String2(200) } };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Metadata!.OriginalTitle);
    }

    [Fact]
    public void Validate_WhenOriginalTitleIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { OriginalTitle = null! } };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Metadata!.OriginalTitle);
    }

    [Fact]
    public void Validate_WhenDescriptionExceeds2000Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { Description = new Faker().Random.String2(2001) } };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Metadata!.Description).WithErrorMessage(Errors.Metadata.DescriptionMustBeMaximum2000CharactersLong.Description);
    }

    [Fact]
    public void Validate_WhenDescriptionIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { Description = new Faker().Random.String2(1500) } };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Metadata!.Description);
    }

    [Fact]
    public void Validate_WhenDescriptionIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { Description = null! } };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Metadata!.Description);
    }

    [Fact]
    public void Validate_WhenDescriptionIsEmpty_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { Description = string.Empty } };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Metadata!.Description);
    }

    [Fact]
    public void Validate_WhenReleaseInfoIsNull_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { ReleaseInfo = null! } };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Metadata!.ReleaseInfo).WithErrorMessage(Errors.Metadata.ReleaseInfoCannotBeNull.Description);
    }

    [Fact]
    public void Validate_WhenOriginalReleaseYearIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { ReleaseInfo = bookCommand.Metadata.ReleaseInfo! with { OriginalReleaseDate = null!, OriginalReleaseYear = new Faker().Random.Int(2000, 2005), ReReleaseYear = new Faker().Random.Int(2005, 2010), ReReleaseDate = null! } } };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Metadata!.ReleaseInfo!.OriginalReleaseYear);
    }

    [Fact]
    public void Validate_WhenOriginalReleaseYearIsLessThan1_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { ReleaseInfo = bookCommand.Metadata.ReleaseInfo! with { OriginalReleaseYear = 0 } } };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Metadata!.ReleaseInfo!.OriginalReleaseYear).WithErrorMessage(Errors.Metadata.OriginalReleaseYearMustBeBetween1And9999.Description);
    }

    [Fact]
    public void Validate_WhenOriginalReleaseYearIsGreaterThan9999_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { ReleaseInfo = bookCommand.Metadata.ReleaseInfo! with { OriginalReleaseYear = 10000 } } };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Metadata!.ReleaseInfo!.OriginalReleaseYear).WithErrorMessage(Errors.Metadata.OriginalReleaseYearMustBeBetween1And9999.Description);
    }

    [Fact]
    public void Validate_WhenReReleaseYearIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
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
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Metadata!.ReleaseInfo!.ReReleaseYear);
    }

    [Fact]
    public void Validate_WhenReReleaseYearIsLessThan1_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { ReleaseInfo = bookCommand.Metadata.ReleaseInfo! with { ReReleaseYear = 0 } } };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Metadata!.ReleaseInfo!.ReReleaseYear).WithErrorMessage(Errors.Metadata.ReReleaseYearMustBeBetween1And9999.Description);
    }

    [Fact]
    public void Validate_WhenReReleaseYearIsGreaterThan9999_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { ReleaseInfo = bookCommand.Metadata.ReleaseInfo! with { ReReleaseYear = 10000 } } };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Metadata!.ReleaseInfo!.ReReleaseYear).WithErrorMessage(Errors.Metadata.ReReleaseYearMustBeBetween1And9999.Description);
    }

    [Fact]
    public void Validate_WhenReleaseCountryIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { ReleaseInfo = bookCommand.Metadata.ReleaseInfo! with { ReleaseCountry = new Faker().Random.String2(2).ToUpper() } } };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Metadata!.ReleaseInfo!.ReleaseCountry);
    }

    [Fact]
    public void Validate_WhenReleaseCountryIsInvalid_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { ReleaseInfo = bookCommand.Metadata.ReleaseInfo! with { ReleaseCountry = new Faker().Random.String2(3) } } };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Metadata!.ReleaseInfo!.ReleaseCountry).WithErrorMessage(Errors.Metadata.CountryCodeMustBe2CharactersLong.Description);
    }

    [Fact]
    public void Validate_WhenReleaseVersionIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { ReleaseInfo = bookCommand.Metadata.ReleaseInfo! with { ReleaseVersion = new Faker().Random.String2(50) } } };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Metadata!.ReleaseInfo!.ReleaseVersion);
    }

    [Fact]
    public void Validate_WhenReleaseVersionExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { ReleaseInfo = bookCommand.Metadata.ReleaseInfo! with { ReleaseVersion = new Faker().Random.String2(51) } } };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Metadata!.ReleaseInfo!.ReleaseVersion).WithErrorMessage(Errors.Metadata.ReleaseVersionMustBeMaximum50CharactersLong.Description);
    }

    [Fact]
    public void Validate_WhenReReleaseYearIsAfterOriginalReleaseYear_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { ReleaseInfo = bookCommand.Metadata.ReleaseInfo! with { OriginalReleaseYear = 2000, ReReleaseYear = 2001, ReReleaseDate = null! } } };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Metadata!.ReleaseInfo!.ReReleaseYear);
    }

    [Fact]
    public void Validate_WhenReReleaseYearIsBeforeOriginalReleaseYear_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { ReleaseInfo = bookCommand.Metadata.ReleaseInfo! with { OriginalReleaseYear = 2001, ReReleaseYear = 2000, ReReleaseDate = null! } } };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Metadata!.ReleaseInfo!.ReReleaseYear).WithErrorMessage(Errors.Metadata.ReReleaseYearCannotBeEarlierThanOriginalReleaseYear.Description);
    }

    [Fact]
    public void Validate_WhenReReleaseDateIsAfterOriginalReleaseDate_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { ReleaseInfo = bookCommand.Metadata.ReleaseInfo! with { OriginalReleaseDate = new DateOnly(2000, 1, 1), ReReleaseDate = new DateOnly(2001, 1, 1) } } };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Metadata!.ReleaseInfo!.ReReleaseDate);
    }

    [Fact]
    public void Validate_WhenReReleaseDateIsBeforeOriginalReleaseDate_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { ReleaseInfo = bookCommand.Metadata.ReleaseInfo! with { OriginalReleaseDate = new DateOnly(2001, 1, 1), ReReleaseDate = new DateOnly(2000, 1, 1) } } };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Metadata!.ReleaseInfo!.ReReleaseDate).WithErrorMessage(Errors.Metadata.ReReleaseDateCannotBeEarlierThanOriginalReleaseDate.Description);
    }

    [Fact]
    public void Validate_WhenGenresIsNull_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { Genres = null! } };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Metadata!.Genres).WithErrorMessage(Errors.Metadata.GenresListCannotBeNull.Description);
    }

    [Fact]
    public void Validate_WhenGenreNameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { Genres = bookCommand.Metadata.Genres!.Select((genre, index) => index == 0 ? genre with { Name = string.Empty } : genre).ToList() } };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor("Metadata.Genres[0].Name").WithErrorMessage(Errors.Metadata.GenreNameCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenGenreNameExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { Genres = bookCommand.Metadata.Genres!.Select((genre, index) => index == 0 ? genre with { Name = new Faker().Random.String2(51) } : genre).ToList() } };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor("Metadata.Genres[0].Name").WithErrorMessage(Errors.Metadata.GenreNameMustBeMaximum50CharactersLong.Description);
    }

    [Fact]
    public void Validate_WhenGenresAreValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with
        {
            Metadata = bookCommand.Metadata! with
            {
                Genres = bookCommand.Metadata.Genres!.Select((genre, index) => index == 0 ? genre with { Name = new Faker().Random.String2(50) } : genre).ToList()
            }
        };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Metadata!.Genres);
    }

    [Fact]
    public void Validate_WhenTagsIsNull_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { Tags = null! } };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Metadata!.Tags).WithErrorMessage(Errors.Metadata.TagsListCannotBeNull.Description);
    }

    [Fact]
    public void Validate_WhenTagNameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { Tags = bookCommand.Metadata.Tags!.Select((tag, index) => index == 0 ? tag with { Name = string.Empty } : tag).ToList() } };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor("Metadata.Tags[0].Name").WithErrorMessage(Errors.Metadata.TagNameCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenTagNameExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { Tags = bookCommand.Metadata.Tags!.Select((tag, index) => index == 0 ? tag with { Name = new Faker().Random.String2(51) } : tag).ToList() } };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor("Metadata.Tags[0].Name").WithErrorMessage(Errors.Metadata.TagNameMustBeMaximum50CharactersLong.Description);
    }

    [Fact]
    public void Validate_WhenTagsAreValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with
        {
            Metadata = bookCommand.Metadata! with
            {
                Tags = bookCommand.Metadata.Tags!.Select((tag, index) => index == 0 ? tag with { Name = new Faker().Random.String2(50) } : tag).ToList()
            }
        };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Metadata!.Tags);
    }

    [Fact]
    public void Validate_WhenLanguageIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { Language = null! } };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Metadata!.Language);
    }

    [Fact]
    public void Validate_WhenLanguageCodeIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { Language = bookCommand.Metadata.Language! with { LanguageCode = string.Empty } } };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Metadata!.Language!.LanguageCode).WithErrorMessage(Errors.Metadata.LanguageCodeCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenLanguageCodeExceeds2Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { Language = bookCommand.Metadata.Language! with { LanguageCode = new Faker().Random.String2(3) } } };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Metadata!.Language!.LanguageCode).WithErrorMessage(Errors.Metadata.LanguageCodeMustBe2CharactersLong.Description);
    }

    [Fact]
    public void Validate_WhenLanguageNameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { Language = bookCommand.Metadata.Language! with { LanguageName = string.Empty } } };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Metadata!.Language!.LanguageName).WithErrorMessage(Errors.Metadata.LanguageNameCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenLanguageNameExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { Language = bookCommand.Metadata.Language! with { LanguageName = new Faker().Random.String2(51) } } };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Metadata!.Language!.LanguageName).WithErrorMessage(Errors.Metadata.LanguageNameMustBeMaximum50CharactersLong.Description);
    }

    [Fact]
    public void Validate_WhenLanguageNativeNameIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { Language = bookCommand.Metadata.Language! with { NativeName = null! } } };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Metadata!.Language!.NativeName);
    }

    [Fact]
    public void Validate_WhenLanguageNativeNameExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { Language = bookCommand.Metadata.Language! with { NativeName = new Faker().Random.String2(51) } } };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Metadata!.Language!.NativeName).WithErrorMessage(Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong.Description);
    }

    [Fact]
    public void Validate_WhenOriginalLanguageIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { OriginalLanguage = null! } };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Metadata!.OriginalLanguage);
    }

    [Fact]
    public void Validate_WhenOriginalLanguageCodeIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { OriginalLanguage = bookCommand.Metadata.OriginalLanguage! with { LanguageCode = string.Empty } } };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Metadata!.OriginalLanguage!.LanguageCode).WithErrorMessage(Errors.Metadata.LanguageCodeCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenOriginalLanguageCodeExceeds2Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { OriginalLanguage = bookCommand.Metadata.OriginalLanguage! with { LanguageCode = new Faker().Random.String2(3) } } };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Metadata!.OriginalLanguage!.LanguageCode).WithErrorMessage(Errors.Metadata.LanguageCodeMustBe2CharactersLong.Description);
    }

    [Fact]
    public void Validate_WhenOriginalLanguageNameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { OriginalLanguage = bookCommand.Metadata.OriginalLanguage! with { LanguageName = string.Empty } } };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Metadata!.OriginalLanguage!.LanguageName).WithErrorMessage(Errors.Metadata.LanguageNameCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenOriginalLanguageNameExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { OriginalLanguage = bookCommand.Metadata.OriginalLanguage! with { LanguageName = new Faker().Random.String2(51) } } };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Metadata!.OriginalLanguage!.LanguageName).WithErrorMessage(Errors.Metadata.LanguageNameMustBeMaximum50CharactersLong.Description);
    }

    [Fact]
    public void Validate_WhenOriginalLanguageNativeNameIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { OriginalLanguage = bookCommand.Metadata.OriginalLanguage! with { NativeName = null! } } };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Metadata!.OriginalLanguage!.NativeName);
    }

    [Fact]
    public void Validate_WhenOriginalLanguageNativeNameExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { OriginalLanguage = bookCommand.Metadata.OriginalLanguage! with { NativeName = new Faker().Random.String2(51) } } };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Metadata!.OriginalLanguage!.NativeName).WithErrorMessage(Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong.Description);
    }

    [Fact]
    public void Validate_WhenCalledWithEmptyPublisher_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { Publisher = null! } };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Metadata!.Publisher);
    }

    [Fact]
    public void Validate_WhenCalledWithValidPublisher_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { Publisher = new Faker().Random.String2(100) } };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Metadata!.Publisher);
    }

    [Fact]
    public void Validate_WhenCalledWithInvalidLengthPublisher_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { Publisher = new Faker().Random.String2(101) } };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Metadata!.Publisher).WithErrorMessage(Errors.WrittenContent.PublisherMustBeMaximum100CharactersLong.Description);
    }

    [Fact]
    public void Validate_WhenPageCountIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { PageCount = null } };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Metadata!.PageCount);
    }

    [Fact]
    public void Validate_WhenPageCountIsZero_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { PageCount = 0 } };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Metadata!.PageCount).WithErrorMessage(Errors.WrittenContent.PageCountMustBeGreaterThanZero.Description);
    }

    [Fact]
    public void Validate_WhenPageCountIsNegative_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { PageCount = -1 } };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Metadata!.PageCount).WithErrorMessage(Errors.WrittenContent.PageCountMustBeGreaterThanZero.Description);
    }

    [Fact]
    public void Validate_WhenPageCountIsPositive_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { PageCount = 100 } };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Metadata!.PageCount);
    }

    [Fact]
    public void Validate_WhenFormatIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Format = null };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Format);
    }

    [Fact]
    public void Validate_WhenFormatIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Format = BookFormat.Hardcover };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Format);
    }

    [Fact]
    public void Validate_WhenFormatIsInvalid_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Format = (BookFormat)99 };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Format).WithErrorMessage(Errors.WrittenContent.UnknownBookFormat.Description);
    }

    [Fact]
    public void Validate_WhenEditionIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Edition = null };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Edition);
    }

    [Fact]
    public void Validate_WhenEditionIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Edition = new Faker().Random.String2(50) };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Edition);
    }

    [Fact]
    public void Validate_WhenEditionExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Edition = new Faker().Random.String2(51) };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Edition).WithErrorMessage(Errors.WrittenContent.EditionMustBeMaximum50CharactersLong.Description);
    }

    [Fact]
    public void Validate_WhenVolumeNumberIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { VolumeNumber = null };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.VolumeNumber);
    }

    [Fact]
    public void Validate_WhenVolumeNumberIsZero_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { VolumeNumber = 0 };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.VolumeNumber).WithErrorMessage(Errors.WrittenContent.VolumeNumberMustBeGreaterThanZero.Description);
    }

    [Fact]
    public void Validate_WhenVolumeNumberIsNegative_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { VolumeNumber = -1 };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.VolumeNumber).WithErrorMessage(Errors.WrittenContent.VolumeNumberMustBeGreaterThanZero.Description);
    }

    [Fact]
    public void Validate_WhenVolumeNumberIsPositive_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { VolumeNumber = 1 };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.VolumeNumber);
    }

    [Fact]
    public void Validate_WhenSeriesIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Series = null };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Series);
    }

    //[Fact]
    //public void Validate_WhenSeriesTitleIsEmpty_ShouldHaveValidationError()
    //{
    //    // Arrange
    //    var bookCommand = _commandBookFixture.CreateCommandBook();
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
    //    var bookCommand = _commandBookFixture.CreateCommandBook();
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
    //    var bookCommand = _commandBookFixture.CreateCommandBook();
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
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { ASIN = new Faker().Random.String2(10) };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ASIN);
    }

    [Fact]
    public void Validate_WhenAsinIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { ASIN = null };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ASIN);
    }

    [Fact]
    public void Validate_WhenAsinIsNotTenCharacters_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { ASIN = new Faker().Random.String2(9) };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ASIN).WithErrorMessage(Errors.WrittenContent.AsinMustBe10CharactersLong.Description);
    }

    [Fact]
    public void Validate_WhenGoodreadsIdIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { GoodreadsId = null };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.GoodreadsId);
    }

    [Fact]
    public void Validate_WhenGoodreadsIdIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { GoodreadsId = "123456789" };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.GoodreadsId);
    }

    [Fact]
    public void Validate_WhenGoodreadsIdIsNonNumeric_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { GoodreadsId = "abc123" };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.GoodreadsId).WithErrorMessage(Errors.WrittenContent.GoodreadsIdMustBeNumeric.Description);
    }

    [Fact]
    public void Validate_WhenGoodreadsIdContainsSpaces_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { GoodreadsId = "123 456" };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.GoodreadsId).WithErrorMessage(Errors.WrittenContent.GoodreadsIdMustBeNumeric.Description);
    }

    [Fact]
    public void Validate_WhenGoodreadsIdContainsSpecialCharacters_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { GoodreadsId = "123-456" };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.GoodreadsId).WithErrorMessage(Errors.WrittenContent.GoodreadsIdMustBeNumeric.Description);
    }

    [Fact]
    public void Validate_WhenLccnIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { LCCN = null };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.LCCN);
    }

    [Fact]
    public void Validate_WhenLccnIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { LCCN = "n78890351" };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.LCCN);
    }

    [Fact]
    public void Validate_WhenLccnHasInvalidFormat_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { LCCN = "invalid123" };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LCCN).WithErrorMessage(Errors.WrittenContent.InvalidLccnFormat.Description);
    }

    [Fact]
    public void Validate_WhenLccnIsTooLong_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { LCCN = new Faker().Random.String2(15) };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LCCN).WithErrorMessage(Errors.WrittenContent.InvalidLccnFormat.Description);
    }

    [Fact]
    public void Validate_WhenLccnIsTooShort_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { LCCN = "n12" };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LCCN).WithErrorMessage(Errors.WrittenContent.InvalidLccnFormat.Description);
    }

    [Fact]
    public void Validate_WhenCalledWithEmptyOclcNumber_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { OCLCNumber = null };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.OCLCNumber);
    }

    [Fact]
    public void Validate_WhenCalledWithValidOclcNumberFormat1_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { OCLCNumber = "ocm12345678" };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.OCLCNumber);
    }

    [Fact]
    public void Validate_WhenCalledWithValidOclcNumberFormat2_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { OCLCNumber = "ocn123456789" };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.OCLCNumber);
    }

    [Fact]
    public void Validate_WhenCalledWithValidOclcNumberFormat3_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { OCLCNumber = "on1234567890" };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.OCLCNumber);
    }

    [Fact]
    public void Validate_WhenCalledWithValidOclcNumberFormat4_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { OCLCNumber = "(OCoLC)1234567890" };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.OCLCNumber);
    }

    [Fact]
    public void Validate_WhenCalledWithValidOclcNumberFormat5_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { OCLCNumber = "12345678" };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.OCLCNumber);
    }

    [Fact]
    public void Validate_WhenCalledWithInvalidOclcNumber_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { OCLCNumber = "invalid_oclc_number" };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.OCLCNumber).WithErrorMessage(Errors.WrittenContent.InvalidOclcFormat.Description);
    }

    [Fact]
    public void Validate_WhenOpenLibraryIdIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { OpenLibraryId = null! };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.OpenLibraryId);
    }

    [Fact]
    public void Validate_WhenOpenLibraryIdIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { OpenLibraryId = "OL123456M" };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.OpenLibraryId);
    }

    [Fact]
    public void Validate_WhenOpenLibraryIdIsInvalid_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { OpenLibraryId = "InvalidID" };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.OpenLibraryId).WithErrorMessage(Errors.WrittenContent.InvalidOpenLibraryId.Description);
    }

    [Fact]
    public void Validate_WhenOpenLibraryIdStartsWithOLButIsInvalidFormat_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { OpenLibraryId = "OL123ABC" };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.OpenLibraryId).WithErrorMessage(Errors.WrittenContent.InvalidOpenLibraryId.Description);
    }

    [Fact]
    public void Validate_WhenOpenLibraryIdHasValidFormatButInvalidSuffix_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { OpenLibraryId = "OL123456X" };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.OpenLibraryId).WithErrorMessage(Errors.WrittenContent.InvalidOpenLibraryId.Description);
    }

    [Fact]
    public void Validate_WhenCalledWithEmptyLibraryThingId_ShouldAddBook()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { LibraryThingId = null };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.LibraryThingId);
    }

    [Fact]
    public void Validate_WhenCalledWithValidLibraryThingId_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { LibraryThingId = new Faker().Random.String2(50) };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.LibraryThingId);
    }

    [Fact]
    public void Validate_WhenCalledWithInvalidLengthLibraryThingId_ShouldReturnBadRequest()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { LibraryThingId = new Faker().Random.String2(51) };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LibraryThingId).WithErrorMessage(Errors.WrittenContent.LibraryThingIdMustBeMaximum50CharactersLong.Description);
    }

    [Fact]
    public void Validate_WhenCalledWithEmptyGoogleBooksId_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { GoogleBooksId = null! };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.GoogleBooksId);
    }

    [Fact]
    public void Validate_WhenCalledWithInvalidLengthGoogleBooksId_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { GoogleBooksId = new Faker().Random.String2(11) };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.GoogleBooksId).WithErrorMessage(Errors.WrittenContent.GoogleBooksIdMustBe12CharactersLong.Description);
    }

    [Fact]
    public void Validate_WhenCalledWithInvalidFormatGoogleBooksId_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { GoogleBooksId = new Faker().Random.String2(11) + " " };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.GoogleBooksId).WithErrorMessage(Errors.WrittenContent.InvalidGoogleBooksIdFormat.Description);
    }

    [Fact]
    public void Validate_WhenCalledWithValidGoogleBooksId_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { GoogleBooksId = new Faker().Random.String2(12, "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_-") };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.GoogleBooksId);
    }

    [Fact]
    public void Validate_WhenCalledWithEmptyBarnesAndNobleId_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { BarnesAndNobleId = null! };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.BarnesAndNobleId);
    }

    [Fact]
    public void Validate_WhenCalledWithInvalidLengthBarnesAndNobleId_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { BarnesAndNobleId = new Faker().Random.String2(11) };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BarnesAndNobleId).WithErrorMessage(Errors.WrittenContent.BarnesAndNoblesIdMustBe10CharactersLong.Description);
    }

    [Fact]
    public void Validate_WhenCalledWithNonNumericBarnesAndNobleId_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { BarnesAndNobleId = new Faker().Random.AlphaNumeric(10) };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BarnesAndNobleId).WithErrorMessage(Errors.WrittenContent.InvalidBarnesAndNoblesIdFormat.Description);
    }

    [Fact]
    public void Validate_WhenCalledWithValidBarnesAndNobleId_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { BarnesAndNobleId = new Faker().Random.Number(1000000000, 999999999).ToString() };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.BarnesAndNobleId);
    }

    [Fact]
    public void AddBook_WhenCalledWithEmptyAppleBooksId_ShouldAddBook()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { AppleBooksId = null };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.AppleBooksId);
    }

    [Fact]
    public void AddBook_WhenCalledWithValidAppleBooksId_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { AppleBooksId = "id123456" };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.AppleBooksId);
    }

    [Fact]
    public void AddBook_WhenCalledWithInvalidAppleBooksId_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { AppleBooksId = "invalid_id" };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AppleBooksId).WithErrorMessage(Errors.WrittenContent.InvalidAppleBooksIdFormat.Description);
    }

    [Fact]
    public void Validate_WhenCalledWithNullIsbns_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { ISBNs = null! };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ISBNs).WithErrorMessage(Errors.WrittenContent.IsbnListCannotBeNull.Description);
    }

    [Fact]
    public void Validate_WhenCalledWithEmptyIsbnValue_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { ISBNs = bookCommand.ISBNs!.Select((isbn, index) => index == 0 ? isbn with { Value = null! } : isbn).ToList() };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor("ISBNs[0].Value").WithErrorMessage(Errors.WrittenContent.IsbnValueCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenCalledWithInvalidIsbn10Value_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { ISBNs = bookCommand.ISBNs!.Select((isbn, index) => index == 0 ? isbn with { Value = new Faker().Random.String2(5), Format = IsbnFormat.Isbn10 } : isbn).ToList() };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor("ISBNs[0].Value").WithErrorMessage(Errors.WrittenContent.InvalidIsbn10Format.Description);
    }

    [Fact]
    public void Validate_WhenCalledWithInvalidIsbn13Value_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { ISBNs = bookCommand.ISBNs!.Select((isbn, index) => index == 0 ? isbn with { Value = new Faker().Random.String2(5), Format = IsbnFormat.Isbn13 } : isbn).ToList() };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor("ISBNs[0].Value").WithErrorMessage(Errors.WrittenContent.InvalidIsbn13Format.Description);
    }

    [Fact]
    public void Validate_WhenCalledWithInvalidIsbnFormat_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { ISBNs = bookCommand.ISBNs!.Select((isbn, index) => index == 0 ? isbn with { Format = (IsbnFormat)99 } : isbn).ToList() };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor("ISBNs[0].Format").WithErrorMessage(Errors.WrittenContent.UnknownIsbnFormat.Description);
    }

    [Fact]
    public void Validate_WhenCalledWithValidIsbn10_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { ISBNs = bookCommand.ISBNs!.Select((isbn, index) => index == 0 ? isbn with { Value = "0-306-40615-2", Format = IsbnFormat.Isbn10 } : isbn).ToList() };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ISBNs![0].Value);
    }

    [Fact]
    public void Validate_WhenCalledWithValidIsbn13_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { ISBNs = bookCommand.ISBNs!.Select((isbn, index) => index == 0 ? isbn with { Value = "978-3-16-148410-0", Format = IsbnFormat.Isbn13 } : isbn).ToList() };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ISBNs![0].Value);
    }

    [Fact]
    public void Validate_WhenCalledWithNullContributors_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Contributors = null! };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Contributors).WithErrorMessage(Errors.MediaContributor.ContributorsListCannotBeNull.Description);
    }

    [Fact]
    public void Validate_WhenCalledWithNullContributorName_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Contributors = bookCommand.Contributors!.Select((contributor, index) => index == 0 ? contributor with { Name = null! } : contributor).ToList() };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor("Contributors[0].Name").WithErrorMessage(Errors.MediaContributor.ContributorNameCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenCalledWithInvalidLengthContributorDisplayName_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Contributors = bookCommand.Contributors!.Select((contributor, index) => index == 0 ? contributor with { Name = contributor.Name! with { DisplayName = new Faker().Random.String2(101) } } : contributor).ToList() };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor("Contributors[0].Name.DisplayName").WithErrorMessage(Errors.MediaContributor.ContributorDisplayNameMustBeMaximum100CharactersLong.Description);
    }

    [Fact]
    public void Validate_WhenCalledWithEmptyContributorDisplayName_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Contributors = bookCommand.Contributors!.Select((contributor, index) => index == 0 ? contributor with { Name = contributor.Name! with { DisplayName = null! } } : contributor).ToList() };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor("Contributors[0].Name.DisplayName").WithErrorMessage(Errors.MediaContributor.ContributorDisplayNameCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenCalledWithInvalidLengthContributorLegalName_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Contributors = bookCommand.Contributors!.Select((contributor, index) => index == 0 ? contributor with { Name = contributor.Name! with { LegalName = new Faker().Random.String2(101) } } : contributor).ToList() };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor("Contributors[0].Name.LegalName").WithErrorMessage(Errors.MediaContributor.ContributorLegalNameMustBeMaximum100CharactersLong.Description);
    }

    [Fact]
    public void Validate_WhenCalledWithNullContributorRole_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Contributors = bookCommand.Contributors!.Select((contributor, index) => index == 0 ? contributor with { Role = null! } : contributor).ToList() };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor("Contributors[0].Role").WithErrorMessage(Errors.MediaContributor.ContributorRoleCannotBeNull.Description);
    }

    [Fact]
    public void Validate_WhenCalledWithEmptyContributorRoleName_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Contributors = bookCommand.Contributors!.Select((contributor, index) => index == 0 ? contributor with { Role = contributor.Role! with { Name = null! } } : contributor).ToList() };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor("Contributors[0].Role.Name").WithErrorMessage(Errors.MediaContributor.RoleNameCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenCalledWithInvalidLengthContributorRoleName_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Contributors = bookCommand.Contributors!.Select((contributor, index) => index == 0 ? contributor with { Role = contributor.Role! with { Name = new Faker().Random.String2(51) } } : contributor).ToList() };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor("Contributors[0].Role.Name").WithErrorMessage(Errors.MediaContributor.RoleNameMustBeMaximum50CharactersLong.Description);
    }

    [Fact]
    public void Validate_WhenCalledWithEmptyContributorRoleCategory_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Contributors = bookCommand.Contributors!.Select((contributor, index) => index == 0 ? contributor with { Role = contributor.Role! with { Category = null! } } : contributor).ToList() };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor("Contributors[0].Role.Category").WithErrorMessage(Errors.MediaContributor.RoleCategoryCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenCalledWithInvalidLengthContributorRoleCategory_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Contributors = bookCommand.Contributors!.Select((contributor, index) => index == 0 ? contributor with { Role = contributor.Role! with { Category = new Faker().Random.String2(51) } } : contributor).ToList() };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor("Contributors[0].Role.Category").WithErrorMessage(Errors.MediaContributor.RoleCategoryMustBeMaximum50CharactersLong.Description);
    }

    [Fact]
    public void Validate_WhenCalledWithNullRatings_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Ratings = null! };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Ratings).WithErrorMessage(Errors.Metadata.RatingsListCannotBeNull.Description);
    }

    [Fact]
    public void Validate_WhenCalledWithNegativeRatingValue_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Ratings = bookCommand.Ratings!.Select((rating, index) => index == 0 ? rating with { Value = -1 } : rating).ToList() };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor("Ratings[0].Value").WithErrorMessage(Errors.Metadata.RatingValueMustBePositive.Description);
    }

    [Fact]
    public void Validate_WhenCalledWithRatingValueGreaterThanMaxValue_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Ratings = bookCommand.Ratings!.Select((rating, index) => index == 0 ? rating with { Value = 6, MaxValue = 5 } : rating).ToList() };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor("Ratings[0].Value").WithErrorMessage(Errors.Metadata.RatingValueCannotBeGreaterThanMaxValue.Description);
    }

    [Fact]
    public void Validate_WhenCalledWithNegativeMaxRatingValue_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Ratings = bookCommand.Ratings!.Select((rating, index) => index == 0 ? rating with { MaxValue = -1 } : rating).ToList() };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor("Ratings[0].MaxValue").WithErrorMessage(Errors.Metadata.RatingMaxValueMustBePositive.Description);
    }

    [Fact]
    public void Validate_WhenCalledWithNegativeVoteCount_ShouldHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Ratings = bookCommand.Ratings!.Select((rating, index) => index == 0 ? rating with { VoteCount = -1 } : rating).ToList() };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldHaveValidationErrorFor("Ratings[0].VoteCount").WithErrorMessage(Errors.Metadata.RatingVoteCountMustBePositive.Description);
    }

    [Fact]
    public void Validate_WhenCalledWithValidRatings_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with
        {
            Ratings = bookCommand.Ratings!.Select((rating, index) => index == 0 ? rating
            with
            { Value = 4, MaxValue = 5, VoteCount = 100 } : rating).ToList()
        };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Ratings);
    }

    [Fact]
    public void Validate_WhenCalledWithNullVoteCount_ShouldNotHaveValidationError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Ratings = bookCommand.Ratings!.Select((rating, index) => index == 0 ? rating with { VoteCount = null } : rating).ToList() };

        // Act
        TestValidationResult<AddBookCommand> result = _validator.TestValidate(bookCommand);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Ratings![0].VoteCount);
    }
}
