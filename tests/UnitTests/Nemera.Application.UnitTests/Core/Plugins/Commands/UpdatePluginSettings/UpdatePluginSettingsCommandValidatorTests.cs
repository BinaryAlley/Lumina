#region ========================================================================= USING =====================================================================================
using Nemera.Application.Core.Plugins.Commands.UpdatePluginSettings;
using Nemera.Application.Fixtures.Core.Plugins.Commands.UpdatePluginSettings;
using Nemera.Application.UnitTests.Common.Setup;
using Nemera.Domain.Common.Errors;
using Nemera.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Application.UnitTests.Core.Plugins.Commands.UpdatePluginSettings;

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
