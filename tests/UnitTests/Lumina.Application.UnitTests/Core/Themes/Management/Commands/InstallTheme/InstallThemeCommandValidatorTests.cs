#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Themes.Management.Commands.InstallTheme;
using Lumina.Application.Fixtures.Core.Themes.Management.Commands.InstallTheme;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
#endregion

namespace Lumina.Application.UnitTests.Core.Themes.Management.Commands.InstallTheme;

/// <summary>
/// Contains unit tests for the <see cref="InstallThemeCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class InstallThemeCommandValidatorTests
{
    private readonly InstallThemeCommandValidator _validator = new();
    private readonly InstallThemeCommandFixture _installThemeCommandFixture = new();

    [Fact]
    public void Validate_WhenArchiveIsNull_ShouldHaveValidationError()
    {
        // Arrange
        InstallThemeCommand command = _installThemeCommandFixture.Create();
        command = command with { Archive = null };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Themes.ThemeArchiveCannotBeNull);
    }

    [Fact]
    public void Validate_WhenArchiveIsProvided_ShouldNotHaveValidationError()
    {
        // Arrange
        InstallThemeCommand command = _installThemeCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Themes.ThemeArchiveCannotBeNull);
    }

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveAnyValidationErrors()
    {
        // Arrange
        InstallThemeCommand command = _installThemeCommandFixture.Create(new MemoryStream());

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
