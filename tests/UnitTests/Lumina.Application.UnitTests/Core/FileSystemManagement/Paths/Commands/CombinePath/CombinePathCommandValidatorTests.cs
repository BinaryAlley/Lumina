#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.FileSystemManagement.Paths.Commands.CombinePath;
using Lumina.Application.Fixtures.Core.FileSystemManagement.Paths.Commands.CombinePath;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.FileSystemManagement.Paths.Commands.CombinePath;

/// <summary>
/// Contains unit tests for the <see cref="CombinePathCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CombinePathCommandValidatorTests
{
    private readonly CombinePathCommandValidator _validator;
    private readonly CombinePathCommandFixture _combinePathCommandFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="CombinePathCommandValidatorTests"/> class.
    /// </summary>
    public CombinePathCommandValidatorTests()
    {
        _validator = new CombinePathCommandValidator();
        _combinePathCommandFixture = new CombinePathCommandFixture();
    }

    [Fact]
    public void Validate_WhenOriginalPathIsNull_ShouldHaveValidationError()
    {
        // Arrange
        CombinePathCommand command = _combinePathCommandFixture.Create();
        command = command with { OriginalPath = null! };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.FileSystemManagement.PathCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenOriginalPathIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        CombinePathCommand command = _combinePathCommandFixture.Create();
        command = command with { OriginalPath = string.Empty };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.FileSystemManagement.PathCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenOriginalPathIsWhitespace_ShouldHaveValidationError()
    {
        // Arrange
        CombinePathCommand command = _combinePathCommandFixture.Create();
        command = command with { OriginalPath = "   " };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.FileSystemManagement.PathCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenNewPathIsNull_ShouldHaveValidationError()
    {
        // Arrange
        CombinePathCommand command = _combinePathCommandFixture.Create();
        command = command with { NewPath = null! };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.FileSystemManagement.PathCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenNewPathIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        CombinePathCommand command = _combinePathCommandFixture.Create();
        command = command with { NewPath = string.Empty };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.FileSystemManagement.PathCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenNewPathIsWhitespace_ShouldHaveValidationError()
    {
        // Arrange
        CombinePathCommand command = _combinePathCommandFixture.Create();
        command = command with { NewPath = "   " };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.FileSystemManagement.PathCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenBothPathsAreValid_ShouldNotHaveValidationError()
    {
        // Arrange
        CombinePathCommand command = _combinePathCommandFixture.Create();
        command = command with { OriginalPath = "/valid/path", NewPath = "new/segment" };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.FileSystemManagement.PathCannotBeEmpty);
        result.ShouldNotHaveValidationError(Errors.FileSystemManagement.PathCannotBeEmpty);
    }
}
