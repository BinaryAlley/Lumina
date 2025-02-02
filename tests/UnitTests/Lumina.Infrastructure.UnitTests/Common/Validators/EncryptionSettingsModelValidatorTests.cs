#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using Lumina.Infrastructure.Common.Errors;
using Lumina.Infrastructure.Common.Models.Configuration;
using Lumina.Infrastructure.Common.Validators;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Infrastructure.UnitTests.Common.Validators;

/// <summary>
/// Contains unit tests for the <see cref="EncryptionSettingsModelValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class EncryptionSettingsModelValidatorTests
{
    private readonly EncryptionSettingsModelValidator _validator;
    private readonly IFixture _fixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="EncryptionSettingsModelValidatorTests"/> class.
    /// </summary>
    public EncryptionSettingsModelValidatorTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        _validator = new();
    }

    [Fact]
    public void EncryptionSettingsModelValidator_WhenValidBase64SecretKeyProvided_ShouldNotHaveValidationError()
    {
        // Arrange
        EncryptionSettingsModel model = _fixture.Build<EncryptionSettingsModel>()
            .With(x => x.SecretKey, "dGhpcyBpcyBhIHZhbGlkIGJhc2U2NCBzdHJpbmc=") // "this is a valid base64 string" in base64
            .Create();

        // Act
        FluentValidation.Results.ValidationResult result = _validator.Validate(model);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void EncryptionSettingsModelValidator_WhenSecretKeyIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        EncryptionSettingsModel model = _fixture.Build<EncryptionSettingsModel>()
            .With(x => x.SecretKey, string.Empty)
            .Create();

        // Act
        FluentValidation.Results.ValidationResult result = _validator.Validate(model);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal(Errors.Configuration.EncryptionSecretKeyCannotBeEmpty.Description, result.Errors[0].ErrorMessage);
    }

    [Theory]
    [InlineData("not-a-base64-string")]
    [InlineData("invalid!base64")]
    [InlineData("123")]
    public void EncryptionSettingsModelValidator_WhenSecretKeyIsNotBase64_ShouldHaveValidationError(string invalidBase64)
    {
        // Arrange
        EncryptionSettingsModel model = _fixture.Build<EncryptionSettingsModel>()
            .With(x => x.SecretKey, invalidBase64)
            .Create();

        // Act
        FluentValidation.Results.ValidationResult result = _validator.Validate(model);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal(Errors.Configuration.EncryptionSecretKeyMustBeABase64String.Description, result.Errors[0].ErrorMessage);
    }
}
