#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Configuration;
using Lumina.Presentation.Web.Common.Primitives;
using Lumina.Presentation.Web.Common.Validators;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Configuration;
using Lumina.Presentation.Web.UnitTests.Common.Setup;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Common.Validators;

/// <summary>
/// Contains unit tests for the <see cref="EncryptionSettingsDtoValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class EncryptionSettingsModelValidatorTests
{
    private const string VALID_BASE64_KEY = "FLYO0QRo6u2VzoFOgNkkEwYNGtqhJ3QGZd7iAHNEJeM=";
    private readonly EncryptionSettingsDtoFixture _encryptionSettingsDtoFixture = new();
    private readonly EncryptionSettingsDtoValidator _validator = new();

    [Fact]
    public void Validate_WhenSecretKeyIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        EncryptionSettingsDto settings = _encryptionSettingsDtoFixture.Create(secretKey: string.Empty);

        // Act
        List<Error> result = _validator.TestValidate(settings);

        // Assert
        result.ShouldHaveValidationError(Error.Validation(description: "Encryption secret key cannot be empty!"));
    }

    [Fact]
    public void Validate_WhenSecretKeyIsNotBase64_ShouldHaveValidationError()
    {
        // Arrange
        EncryptionSettingsDto settings = _encryptionSettingsDtoFixture.Create(secretKey: "not-base64-key!");

        // Act
        List<Error> result = _validator.TestValidate(settings);

        // Assert
        result.ShouldHaveValidationError(Error.Validation(description: "Encryption secret key must be a base64 string!"));
    }

    [Fact]
    public void Validate_WhenSecretKeyIsValidBase64_ShouldNotHaveValidationError()
    {
        // Arrange
        EncryptionSettingsDto settings = _encryptionSettingsDtoFixture.Create(secretKey: VALID_BASE64_KEY);

        // Act
        List<Error> result = _validator.TestValidate(settings);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
