#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Plugins.Queries.GetPluginSettings;
using Lumina.Application.Fixtures.Core.Plugins.Queries.GetPluginSettings;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.Plugins.Queries.GetPluginSettings;

/// <summary>
/// Contains unit tests for the <see cref="GetPluginSettingsQueryValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPluginSettingsQueryValidatorTests
{
    private readonly GetPluginSettingsQueryValidator _validator = new();
    private readonly GetPluginSettingsQueryFixture _getPluginSettingsQueryFixture = new();

    [Fact]
    public void Validate_WhenPluginIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        GetPluginSettingsQuery query = _getPluginSettingsQueryFixture.Create();
        query = query with { PluginId = Guid.Empty };

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Plugins.PluginIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenPluginIdIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        GetPluginSettingsQuery query = _getPluginSettingsQueryFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
