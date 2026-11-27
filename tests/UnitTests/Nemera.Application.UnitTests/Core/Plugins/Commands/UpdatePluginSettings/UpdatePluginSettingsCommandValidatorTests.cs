#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Plugins.Commands.UpdatePluginSettings;
using Lumina.Application.Fixtures.Core.Plugins.Commands.UpdatePluginSettings;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.Plugins.Commands.UpdatePluginSettings;

/// <summary>
/// Contains unit tests for the <see cref="UpdatePluginSettingsCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdatePluginSettingsCommandValidatorTests
{
    private readonly UpdatePluginSettingsCommandValidator _validator = new();
    private readonly UpdatePluginSettingsCommandFixture _updatePluginSettingsCommandFixture = new();

    [Fact]
    public void Validate_WhenPluginIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        UpdatePluginSettingsCommand command = _updatePluginSettingsCommandFixture.Create();
        command = command with { PluginId = Guid.Empty };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Plugins.PluginIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenPluginIdIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdatePluginSettingsCommand command = _updatePluginSettingsCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
