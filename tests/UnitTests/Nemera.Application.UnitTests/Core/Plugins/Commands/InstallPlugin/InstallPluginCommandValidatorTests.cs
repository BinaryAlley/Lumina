#region ========================================================================= USING =====================================================================================
using Nemera.Application.Core.Plugins.Commands.InstallPlugin;
using Nemera.Application.Fixtures.Core.Plugins.Commands.InstallPlugin;
using Nemera.Application.UnitTests.Common.Setup;
using Nemera.Domain.Common.Errors;
using Nemera.Domain.Common.Primitives;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Application.UnitTests.Core.Plugins.Commands.InstallPlugin;

/// <summary>
/// Contains unit tests for the <see cref="InstallPluginCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class InstallPluginCommandValidatorTests
{
    private readonly InstallPluginCommandValidator _validator = new();
    private readonly InstallPluginCommandFixture _installPluginCommandFixture = new();

    [Fact]
    public void Validate_WhenArchiveIsNull_ShouldHaveValidationError()
    {
        // Arrange
        InstallPluginCommand command = _installPluginCommandFixture.Create() with { Archive = null };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Plugins.PluginArchiveCannotBeNull);
    }

    [Fact]
    public void Validate_WhenFileNameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        InstallPluginCommand command = _installPluginCommandFixture.Create() with { FileName = string.Empty };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Plugins.PluginFileNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        InstallPluginCommand command = _installPluginCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
