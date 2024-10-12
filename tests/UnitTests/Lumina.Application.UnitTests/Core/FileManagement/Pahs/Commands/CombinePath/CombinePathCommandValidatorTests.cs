#region ========================================================================= USING =====================================================================================
using FluentValidation.TestHelper;
using Lumina.Application.Core.FileManagement.Paths.Commands.CombinePath;
using Lumina.Application.UnitTests.Core.FileManagement.Pahs.Commands.CombinePath.Fixtures;
using Lumina.Domain.Common.Errors;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.FileManagement.Pahs.Commands.CombinePath;

/// <summary>
/// Contains unit tests for the <see cref="CombinePathCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CombinePathCommandValidatorTests
{
    private readonly CombinePathCommandValidator _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="CombinePathCommandValidatorTests"/> class.
    /// </summary>
    public CombinePathCommandValidatorTests()
    {
        _validator = new CombinePathCommandValidator();
    }

    [Fact]
    public void Validate_WhenOriginalPathIsNull_ShouldHaveValidationError()
    {
        // Arrange
        CombinePathCommand command = CombinePathCommandFixture.CreateCombinePathCommand();
        command = command with { OriginalPath = null! };

        // Act
        TestValidationResult<CombinePathCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.OriginalPath)
            .WithErrorMessage(Errors.FileManagement.PathCannotBeEmpty.Code);
    }

    [Fact]
    public void Validate_WhenOriginalPathIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        CombinePathCommand command = CombinePathCommandFixture.CreateCombinePathCommand();
        command = command with { OriginalPath = string.Empty };

        // Act
        TestValidationResult<CombinePathCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.OriginalPath)
            .WithErrorMessage(Errors.FileManagement.PathCannotBeEmpty.Code);
    }

    [Fact]
    public void Validate_WhenOriginalPathIsWhitespace_ShouldHaveValidationError()
    {
        // Arrange
        CombinePathCommand command = CombinePathCommandFixture.CreateCombinePathCommand();
        command = command with { OriginalPath = "   " };

        // Act
        TestValidationResult<CombinePathCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.OriginalPath)
            .WithErrorMessage(Errors.FileManagement.PathCannotBeEmpty.Code);
    }

    [Fact]
    public void Validate_WhenNewPathIsNull_ShouldHaveValidationError()
    {
        // Arrange
        CombinePathCommand command = CombinePathCommandFixture.CreateCombinePathCommand();
        command = command with { NewPath = null! };

        // Act
        TestValidationResult<CombinePathCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPath)
            .WithErrorMessage(Errors.FileManagement.PathCannotBeEmpty.Code);
    }

    [Fact]
    public void Validate_WhenNewPathIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        CombinePathCommand command = CombinePathCommandFixture.CreateCombinePathCommand();
        command = command with { NewPath = string.Empty };

        // Act
        TestValidationResult<CombinePathCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPath)
            .WithErrorMessage(Errors.FileManagement.PathCannotBeEmpty.Code);
    }

    [Fact]
    public void Validate_WhenNewPathIsWhitespace_ShouldHaveValidationError()
    {
        // Arrange
        CombinePathCommand command = CombinePathCommandFixture.CreateCombinePathCommand();
        command = command with { NewPath = "   " };

        // Act
        TestValidationResult<CombinePathCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPath)
            .WithErrorMessage(Errors.FileManagement.PathCannotBeEmpty.Code);
    }

    [Fact]
    public void Validate_WhenBothPathsAreValid_ShouldNotHaveValidationError()
    {
        // Arrange
        CombinePathCommand command = CombinePathCommandFixture.CreateCombinePathCommand();
        command = command with { OriginalPath = "/valid/path", NewPath = "new/segment" };

        // Act
        TestValidationResult<CombinePathCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.OriginalPath);
        result.ShouldNotHaveValidationErrorFor(x => x.NewPath);
    }
}
