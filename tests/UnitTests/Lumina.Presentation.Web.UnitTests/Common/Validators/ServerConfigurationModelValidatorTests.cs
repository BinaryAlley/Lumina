#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Configuration;
using Lumina.Presentation.Web.Common.Primitives;
using Lumina.Presentation.Web.Common.Validators;
using Lumina.Presentation.Web.UnitTests.Common.Setup;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Common.Validators;

/// <summary>
/// Contains unit tests for the <see cref="ServerConfigurationDtoValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ServerConfigurationModelValidatorTests
{
    private readonly ServerConfigurationDtoValidator _validator = new();

    [Fact]
    public void Validate_WhenBaseAddressIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        ServerConfigurationDto configuration = new() { ApiVersion = '1', BaseAddress = string.Empty, Port = 5214 };

        // Act
        List<Error> result = _validator.TestValidate(configuration);

        // Assert
        result.ShouldHaveValidationError(Error.Validation(description: "Base address cannot be empty!"));
    }

    [Theory]
    [InlineData((ushort)0)] // minimum allowed value
    [InlineData((ushort)65535)] // maximum allowed value
    public void Validate_WhenPortIsAtBoundary_ShouldNotHaveValidationError(ushort port)
    {
        // Arrange
        ServerConfigurationDto configuration = new() { ApiVersion = '1', BaseAddress = "http://localhost", Port = port };

        // Act
        List<Error> result = _validator.TestValidate(configuration);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData((char)0)] // minimum allowed API version
    [InlineData((char)255)] // maximum allowed API version
    public void Validate_WhenApiVersionIsAtBoundary_ShouldNotHaveValidationError(char apiVersion)
    {
        // Arrange
        ServerConfigurationDto configuration = new() { ApiVersion = apiVersion, BaseAddress = "http://localhost", Port = 5214 };

        // Act
        List<Error> result = _validator.TestValidate(configuration);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenConfigurationIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        ServerConfigurationDto configuration = new() { ApiVersion = '1', BaseAddress = "http://localhost", Port = 5214 };

        // Act
        List<Error> result = _validator.TestValidate(configuration);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
