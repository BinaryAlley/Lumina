#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Themes.Management.Commands.DeleteTheme;
using Lumina.Application.Fixtures.Core.Themes.Management.Commands.DeleteTheme;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.Themes.Management.Commands.DeleteTheme;

/// <summary>
/// Contains unit tests for the <see cref="DeleteThemeCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteThemeCommandValidatorTests
{
    private readonly DeleteThemeCommandValidator _validator = new();
    private readonly DeleteThemeCommandFixture _deleteThemeCommandFixture = new();

    [Fact]
    public void Validate_WhenThemeIdIsNull_ShouldHaveValidationError()
    {
        // Arrange
        DeleteThemeCommand command = _deleteThemeCommandFixture.Create();
        command = command with { ThemeId = null };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Themes.ThemeIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenThemeIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        DeleteThemeCommand command = _deleteThemeCommandFixture.Create(themeId: string.Empty);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Themes.ThemeIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenThemeIdIsWhiteSpace_ShouldHaveValidationError()
    {
        // Arrange
        DeleteThemeCommand command = _deleteThemeCommandFixture.Create(themeId: "   ");

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Themes.ThemeIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenThemeIdIsValid_ShouldNotHaveAnyValidationErrors()
    {
        // Arrange
        DeleteThemeCommand command = _deleteThemeCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
