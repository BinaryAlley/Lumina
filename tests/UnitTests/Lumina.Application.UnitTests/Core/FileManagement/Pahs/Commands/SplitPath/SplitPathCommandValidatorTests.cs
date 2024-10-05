#region ========================================================================= USING =====================================================================================
using FluentValidation.TestHelper;
using Lumina.Application.Core.FileManagement.Paths.Commands.SplitPath;
using Lumina.Application.UnitTests.Core.FileManagement.Pahs.Commands.SplitPath.Fixtures;
using Lumina.Domain.Common.Errors;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.FileManagement.Pahs.Commands.SplitPath;

/// <summary>
/// Contains unit tests for the <see cref="SplitPathCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SplitPathCommandValidatorTests
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly SplitPathCommandValidator _validator;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="SplitPathCommandValidatorTests"/> class.
    /// </summary>
    public SplitPathCommandValidatorTests()
    {
        _validator = new SplitPathCommandValidator();
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
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
            .WithErrorMessage(Errors.FileManagement.PathCannotBeEmpty.Code);
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
            .WithErrorMessage(Errors.FileManagement.PathCannotBeEmpty.Code);
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
            .WithErrorMessage(Errors.FileManagement.PathCannotBeEmpty.Code);
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
    #endregion
}
