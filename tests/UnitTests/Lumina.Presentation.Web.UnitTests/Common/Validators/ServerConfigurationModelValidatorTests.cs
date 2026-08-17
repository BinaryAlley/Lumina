#region ========================================================================= USING =====================================================================================
using FluentValidation;
using FluentValidation.Results;
using Lumina.Presentation.Web.Common.DTO.Configuration;
using Lumina.Presentation.Web.Common.Validators;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Common.Validators;

/// <summary>
/// Contains unit tests for the <see cref="ServerConfigurationModelValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ServerConfigurationModelValidatorTests
{
    private readonly ServerConfigurationModelValidator _validator = new();

    [Fact]
    public void Validate_WhenBaseAddressIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        ServerConfigurationDto configuration = new() { ApiVersion = '1', BaseAddress = string.Empty, Port = 5214 };

        // Act
        ValidationResult result = _validator.Validate(configuration);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(ServerConfigurationDto.BaseAddress) && error.ErrorMessage == "Base address cannot be empty!");
    }

    [Theory]
    [InlineData((ushort)0)] // minimum allowed value
    [InlineData((ushort)65535)] // maximum allowed value
    public void Validate_WhenPortIsAtBoundary_ShouldNotHaveValidationError(ushort port)
    {
        // Arrange
        ServerConfigurationDto configuration = new() { ApiVersion = '1', BaseAddress = "http://localhost", Port = port };

        // Act
        ValidationResult result = _validator.Validate(configuration);

        // Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData((char)0)] // minimum allowed API version
    [InlineData((char)255)] // maximum allowed API version
    public void Validate_WhenApiVersionIsAtBoundary_ShouldNotHaveValidationError(char apiVersion)
    {
        // Arrange
        ServerConfigurationDto configuration = new() { ApiVersion = apiVersion, BaseAddress = "http://localhost", Port = 5214 };

        // Act
        ValidationResult result = _validator.Validate(configuration);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WhenConfigurationIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        ServerConfigurationDto configuration = new() { ApiVersion = '1', BaseAddress = "http://localhost", Port = 5214 };

        // Act
        ValidationResult result = _validator.Validate(configuration);

        // Assert
        Assert.True(result.IsValid);
    }
}
