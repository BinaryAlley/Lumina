#region ========================================================================= USING =====================================================================================
using FluentValidation.TestHelper;
using Lumina.Application.Core.FileSystemManagement.Paths.Commands.SplitPath;
using Lumina.Application.UnitTests.Core.FileSystemManagement.Pahs.Commands.SplitPath.Fixtures;
using Lumina.Domain.Common.Errors;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.FileSystemManagement.Pahs.Commands.SplitPath;

/// <summary>
/// Contains unit tests for the <see cref="SplitPathCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SplitPathCommandValidatorTests
{
    private readonly SplitPathCommandValidator _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="SplitPathCommandValidatorTests"/> class.
    /// </summary>
    public SplitPathCommandValidatorTests()
    {
        _validator = new SplitPathCommandValidator();
    }

    [Fact]
    public void Validate_WhenPathIsNull_ShouldHaveValidationError()
    {
        // Arrange
        SplitPathCommand command = SplitPathCommandFixture.CreateSplitPathCommand();
        command = command with { Path = null! };

        // Act
        TestValidationResult<SplitPathCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Path)
            .WithErrorMessage(Errors.FileSystemManagement.PathCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenPathIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        SplitPathCommand command = SplitPathCommandFixture.CreateSplitPathCommand();
        command = command with { Path = string.Empty };

        // Act
        TestValidationResult<SplitPathCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Path)
            .WithErrorMessage(Errors.FileSystemManagement.PathCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenPathIsWhitespace_ShouldHaveValidationError()
    {
        // Arrange
        SplitPathCommand command = SplitPathCommandFixture.CreateSplitPathCommand();
        command = command with { Path = "   " };

        // Act
        TestValidationResult<SplitPathCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Path)
            .WithErrorMessage(Errors.FileSystemManagement.PathCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenPathIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        SplitPathCommand command = SplitPathCommandFixture.CreateSplitPathCommand();
        command = command with { Path = "/valid/path" };

        // Act
        TestValidationResult<SplitPathCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Path);
    }
}
