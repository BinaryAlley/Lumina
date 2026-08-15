#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.Management.Commands.AddLibrary;
using Lumina.Application.Fixtures.Core.MediaLibrary.Management.Commands.AddLibrary;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
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
