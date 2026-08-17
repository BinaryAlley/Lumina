#region ========================================================================= USING =====================================================================================
using FluentValidation;
using FluentValidation.Results;
using Lumina.Presentation.Web.Common.DTO.Configuration;
using Lumina.Presentation.Web.Common.Validators;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Common.Validators;

/// <summary>
/// Contains unit tests for the <see cref="EncryptionSettingsModelValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class EncryptionSettingsModelValidatorTests
{
    private const string VALID_BASE64_KEY = "FLYO0QRo6u2VzoFOgNkkEwYNGtqhJ3QGZd7iAHNEJeM=";
    private readonly EncryptionSettingsModelValidator _validator = new();

    [Fact]
    public void Validate_WhenSecretKeyIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        EncryptionSettingsDto settings = new() { SecretKey = string.Empty };

        // Act
        ValidationResult result = _validator.Validate(settings);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(EncryptionSettingsDto.SecretKey) && error.ErrorMessage == "Encryption secret key cannot be empty!");
    }

    [Fact]
    public void Validate_WhenSecretKeyIsNotBase64_ShouldHaveValidationError()
    {
        // Arrange
        EncryptionSettingsDto settings = new() { SecretKey = "not-base64-key!" };

        // Act
        ValidationResult result = _validator.Validate(settings);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(EncryptionSettingsDto.SecretKey) && error.ErrorMessage == "Encryption secret key must be a base64 string!");
    }

    [Fact]
    public void Validate_WhenSecretKeyIsValidBase64_ShouldNotHaveValidationError()
    {
        // Arrange
        EncryptionSettingsDto settings = new() { SecretKey = VALID_BASE64_KEY };

        // Act
        ValidationResult result = _validator.Validate(settings);

        // Assert
        Assert.True(result.IsValid);
    }
}
