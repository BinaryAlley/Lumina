#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Models.DTO.Configuration;
using Lumina.Domain.Common.Primitives;
using Lumina.Infrastructure.Common.Errors;
using Lumina.Infrastructure.Common.Validators;
using Lumina.Infrastructure.Fixtures.Common.Models.DTO.Configuration;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Infrastructure.UnitTests.Common.Validators;

/// <summary>
/// Contains unit tests for the <see cref="PluginsSettingsDtoValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PluginsSettingsDtoValidatorTests
{
    private readonly PluginsSettingsDtoValidator _validator = new();
    private readonly PluginsSettingsDtoFixture _pluginsSettingsDtoFixture = new();

    [Fact]
    public void Validate_WhenDirectoryProvided_ShouldNotHaveValidationError()
    {
        // Arrange
        PluginsSettingsDto model = _pluginsSettingsDtoFixture.Create(directory: "/path/to/plugins");

        // Act
        List<Error> result = _validator.Validate(model);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_WhenDirectoryIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        PluginsSettingsDto model = _pluginsSettingsDtoFixture.Create(directory: string.Empty);

        // Act
        List<Error> result = _validator.Validate(model);

        // Assert
        Assert.Single(result);
        Assert.Equal(Errors.Configuration.PluginsDirectoryCannotBeEmpty.Description, result[0].Description);
    }

    [Fact]
    public void Validate_WhenDirectoryIsWhitespace_ShouldHaveValidationError()
    {
        // Arrange
        PluginsSettingsDto model = _pluginsSettingsDtoFixture.Create(directory: "   ");

        // Act
        List<Error> result = _validator.Validate(model);

        // Assert
        Assert.Single(result);
        Assert.Equal(Errors.Configuration.PluginsDirectoryCannotBeEmpty.Description, result[0].Description);
    }
}
