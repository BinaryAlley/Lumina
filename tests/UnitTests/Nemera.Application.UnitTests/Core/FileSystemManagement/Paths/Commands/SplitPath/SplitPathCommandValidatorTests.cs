#region ========================================================================= USING =====================================================================================
using Nemera.Application.Core.FileSystemManagement.Paths.Commands.SplitPath;
using Nemera.Application.Fixtures.Core.FileSystemManagement.Paths.Commands.SplitPath;
using Nemera.Application.UnitTests.Common.Setup;
using Nemera.Domain.Common.Errors;
using Nemera.Domain.Common.Primitives;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Application.UnitTests.Core.FileSystemManagement.Paths.Commands.SplitPath;

/// <summary>
/// Contains unit tests for the <see cref="SplitPathCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SplitPathCommandValidatorTests
{
    private readonly SplitPathCommandValidator _validator = new();
    private readonly SplitPathCommandFixture _splitPathCommandFixture = new();

    [Fact]
    public void Validate_WhenPathIsNull_ShouldHaveValidationError()
    {
        // Arrange
        SplitPathCommand command = _splitPathCommandFixture.Create();
        command = command with { Path = null! };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.FileSystemManagement.PathCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenPathIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        SplitPathCommand command = _splitPathCommandFixture.Create();
        command = command with { Path = string.Empty };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.FileSystemManagement.PathCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenPathIsWhitespace_ShouldHaveValidationError()
    {
        // Arrange
        SplitPathCommand command = _splitPathCommandFixture.Create();
        command = command with { Path = "   " };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.FileSystemManagement.PathCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenPathIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        SplitPathCommand command = _splitPathCommandFixture.Create();
        command = command with { Path = "/valid/path" };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.FileSystemManagement.PathCannotBeEmpty);
    }
}
