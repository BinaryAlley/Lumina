#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.MediaLibrary.Management;
using Lumina.Application.Core.MediaLibrary.Management.Commands.AddLibrary;
using Lumina.Application.Fixtures.Common.DTO.MediaLibrary.Management;
using Lumina.Application.Fixtures.Core.MediaLibrary.Management.Commands.AddLibrary;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.Management.Commands.AddLibrary;

/// <summary>
/// Contains unit tests for the <see cref="AddLibraryCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddLibraryCommandValidatorTests
{
    private readonly AddLibraryCommandValidator _validator = new();
    private readonly AddLibraryCommandFixture _addLibraryCommandFixture = new();
    private readonly LibraryPathTemplatePartDtoFixture _libraryPathTemplatePartDtoFixture = new();

    [Fact]
    public void Validate_WhenLibraryTypeIsNull_ShouldHaveValidationError()
    {
        // Arrange
        AddLibraryCommand command = _addLibraryCommandFixture.Create();
        command = command with { LibraryType = null! };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.LibraryTypeCannotBeNull);
    }

    [Fact]
    public void Validate_WhenLibraryTypeIsUnknown_ShouldHaveValidationError()
    {
        // Arrange
        AddLibraryCommand command = _addLibraryCommandFixture.Create();
        command = command with { LibraryType = "UnknownLibraryType" };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.UnknownLibraryType);
    }

    [Fact]
    public void Validate_WhenLibraryTypeIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddLibraryCommand command = _addLibraryCommandFixture.Create();
        command = command with { LibraryType = "Book" };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Library.UnknownLibraryType);
    }

    [Fact]
    public void Validate_WhenContentLocationsIsNull_ShouldHaveValidationError()
    {
        // Arrange
        AddLibraryCommand command = _addLibraryCommandFixture.Create();
        command = command with { ContentLocations = null! };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.PathsListCannotBeNull);
    }

    [Fact]
    public void Validate_WhenContentLocationsIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        AddLibraryCommand command = _addLibraryCommandFixture.Create();
        command = command with { ContentLocations = [] };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.PathsListCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenContentLocationIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        AddLibraryCommand command = _addLibraryCommandFixture.Create();
        command = command with { ContentLocations = [string.Empty] };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.FileSystemManagement.PathCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenContentLocationExceeds260Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddLibraryCommand command = _addLibraryCommandFixture.Create();
        command = command with { ContentLocations = [new string('a', 261)] };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.FileSystemManagement.PathMustBeMaximum260CharactersLong);
    }

    [Fact]
    public void Validate_WhenContentLocationsAreValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddLibraryCommand command = _addLibraryCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Library.PathsListCannotBeNull);
        result.ShouldNotHaveValidationError(Errors.FileSystemManagement.PathCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTitleIsNull_ShouldHaveValidationError()
    {
        // Arrange
        AddLibraryCommand command = _addLibraryCommandFixture.Create();
        command = command with { Title = null! };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.TitleCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTitleIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        AddLibraryCommand command = _addLibraryCommandFixture.Create();
        command = command with { Title = string.Empty };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.TitleCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTitleIsWhitespace_ShouldHaveValidationError()
    {
        // Arrange
        AddLibraryCommand command = _addLibraryCommandFixture.Create();
        command = command with { Title = "   " };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.TitleCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTitleExceeds255Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddLibraryCommand command = _addLibraryCommandFixture.Create();
        command = command with { Title = new string('a', 256) };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.TitleMustBeMaximum255CharactersLong);
    }

    [Fact]
    public void Validate_WhenTitleIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddLibraryCommand command = _addLibraryCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Library.TitleCannotBeEmpty);
        result.ShouldNotHaveValidationError(Errors.Library.TitleMustBeMaximum255CharactersLong);
    }

    [Fact]
    public void Validate_WhenPathTemplatePartsIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddLibraryCommand command = _addLibraryCommandFixture.Create(pathTemplateParts: null);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Library.PathTemplatePartKindNotSupportedForLibraryType);
        result.ShouldNotHaveValidationError(Errors.Library.PathTemplateLiteralCannotBeEmpty);
    }

    [Theory]
    [InlineData(null)] // an absent kind
    [InlineData("")] // an empty kind
    [InlineData("   ")] // a whitespace kind
    public void Validate_WhenPathTemplatePartKindIsNullOrEmpty_ShouldHaveValidationError(string? kind)
    {
        // Arrange
        LibraryPathTemplatePartDto part = _libraryPathTemplatePartDtoFixture.Create() with { Kind = kind };
        AddLibraryCommand command = _addLibraryCommandFixture.Create(pathTemplateParts: [part]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.PathTemplatePartKindNotSupportedForLibraryType);
    }

    [Theory]
    [InlineData(null)] // an absent representation
    [InlineData("")] // an empty representation
    [InlineData("   ")] // a whitespace representation
    public void Validate_WhenNonSeparatorPartRepresentationIsNullOrEmpty_ShouldHaveValidationError(string? representation)
    {
        // Arrange
        LibraryPathTemplatePartDto part = _libraryPathTemplatePartDtoFixture.Create(kind: nameof(LibraryPathPartKind.Artist)) with { Representation = representation };
        AddLibraryCommand command = _addLibraryCommandFixture.Create(pathTemplateParts: [part]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.PathTemplateLiteralCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenSeparatorPartRepresentationIsEmpty_ShouldNotHaveValidationError()
    {
        // Arrange
        LibraryPathTemplatePartDto part = _libraryPathTemplatePartDtoFixture.Create(
            kind: nameof(LibraryPathPartKind.Separator),
            representation: string.Empty);
        AddLibraryCommand command = _addLibraryCommandFixture.Create(pathTemplateParts: [part]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Library.PathTemplateLiteralCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenPathTemplatePartsAreValid_ShouldNotHaveValidationError()
    {
        // Arrange
        LibraryPathTemplatePartDto literalPart = _libraryPathTemplatePartDtoFixture.Create(
            kind: nameof(LibraryPathPartKind.Literal),
            representation: "Various Artists");
        LibraryPathTemplatePartDto separatorPart = _libraryPathTemplatePartDtoFixture.Create(
            kind: nameof(LibraryPathPartKind.Separator),
            representation: string.Empty);
        LibraryPathTemplatePartDto typedPart = _libraryPathTemplatePartDtoFixture.Create(
            kind: nameof(LibraryPathPartKind.Artist),
            representation: "{0}");
        AddLibraryCommand command = _addLibraryCommandFixture.Create(pathTemplateParts: [literalPart, separatorPart, typedPart]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddLibraryCommand command = _addLibraryCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
