#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Themes.Management.Commands.SetCurrentTheme;
using Lumina.Application.Fixtures.Core.Themes.Management.Commands.SetCurrentTheme;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.Themes.Management.Commands.SetCurrentTheme;

/// <summary>
/// Contains unit tests for the <see cref="SetCurrentThemeCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SetCurrentThemeCommandValidatorTests
{
    private readonly SetCurrentThemeCommandValidator _validator = new();
    private readonly SetCurrentThemeCommandFixture _setCurrentThemeCommandFixture = new();

    [Fact]
    public void Validate_WhenThemeIdIsNull_ShouldHaveValidationError()
    {
        // Arrange
        SetCurrentThemeCommand command = _setCurrentThemeCommandFixture.Create();
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
        SetCurrentThemeCommand command = _setCurrentThemeCommandFixture.Create(themeId: string.Empty);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Themes.ThemeIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenThemeIdIsWhiteSpace_ShouldHaveValidationError()
    {
        // Arrange
        SetCurrentThemeCommand command = _setCurrentThemeCommandFixture.Create(themeId: "   ");

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Themes.ThemeIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenThemeIdIsValid_ShouldNotHaveAnyValidationErrors()
    {
        // Arrange
        SetCurrentThemeCommand command = _setCurrentThemeCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
