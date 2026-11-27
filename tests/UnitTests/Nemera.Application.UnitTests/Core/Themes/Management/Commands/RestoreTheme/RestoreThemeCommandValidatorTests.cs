#region ========================================================================= USING =====================================================================================
using Nemera.Application.Core.Themes.Management.Commands.RestoreTheme;
using Nemera.Application.Fixtures.Core.Themes.Management.Commands.RestoreTheme;
using Nemera.Application.UnitTests.Common.Setup;
using Nemera.Domain.Common.Errors;
using Nemera.Domain.Common.Primitives;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Application.UnitTests.Core.Themes.Management.Commands.RestoreTheme;

/// <summary>
/// Contains unit tests for the <see cref="RestoreThemeCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RestoreThemeCommandValidatorTests
{
    private readonly RestoreThemeCommandValidator _validator = new();
    private readonly RestoreThemeCommandFixture _restoreThemeCommandFixture = new();

    [Fact]
    public void Validate_WhenThemeIdIsNull_ShouldHaveValidationError()
    {
        // Arrange
        RestoreThemeCommand command = _restoreThemeCommandFixture.Create();
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
        RestoreThemeCommand command = _restoreThemeCommandFixture.Create(themeId: string.Empty);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Themes.ThemeIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenThemeIdIsValid_ShouldNotHaveValidationErrors()
    {
        // Arrange
        RestoreThemeCommand command = _restoreThemeCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        Assert.Empty(result);
    }
}
