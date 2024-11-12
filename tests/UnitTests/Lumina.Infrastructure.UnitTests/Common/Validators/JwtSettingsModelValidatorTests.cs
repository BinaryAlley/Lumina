#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using FluentAssertions;
using Lumina.Infrastructure.Common.Errors;
using Lumina.Infrastructure.Common.Models.Configuration;
using Lumina.Infrastructure.Common.Validators;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Infrastructure.UnitTests.Common.Validators;

/// <summary>
/// Contains unit tests for the <see cref="JwtSettingsModelValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class JwtSettingsModelValidatorTests
{
    private readonly JwtSettingsModelValidator _validator;
    private readonly IFixture _fixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="JwtSettingsModelValidatorTests"/> class.
    /// </summary>
    public JwtSettingsModelValidatorTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        _validator = new();
    }

    [Fact]
    public void JwtSettingsModelValidator_WhenAllPropertiesValid_ShouldNotHaveValidationError()
    {
        // Arrange
        JwtSettingsModel model = _fixture.Build<JwtSettingsModel>()
            .With(x => x.SecretKey, "this-is-a-very-long-secret-key-for-testing")
            .With(x => x.ExpiryMinutes, 30)
            .With(x => x.Issuer, "test-issuer")
            .With(x => x.Audience, "test-audience")
            .Create();

        // Act
        FluentValidation.Results.ValidationResult result = _validator.Validate(model);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void JwtSettingsModelValidator_WhenSecretKeyIsEmpty_ShouldHaveValidationErrors()
    {
        // Arrange
        JwtSettingsModel model = _fixture.Build<JwtSettingsModel>()
            .With(x => x.SecretKey, string.Empty)
            .With(x => x.ExpiryMinutes, 30)
            .With(x => x.Issuer, "test-issuer")
            .With(x => x.Audience, "test-audience")
            .Create();

        // Act
        FluentValidation.Results.ValidationResult result = _validator.Validate(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
        result.Errors.Select(e => e.ErrorMessage).Should().Contain(new[]
        {
            Errors.Configuration.JwtSecretKeyCannotBeEmpty.Description,
            Errors.Configuration.JwtSecretKeyTooShort.Description
        });
    }

    [Fact]
    public void JwtSettingsModelValidator_WhenSecretKeyTooShort_ShouldHaveValidationError()
    {
        // Arrange
        JwtSettingsModel model = _fixture.Build<JwtSettingsModel>()
            .With(x => x.SecretKey, "short-key")
            .With(x => x.ExpiryMinutes, 30)
            .With(x => x.Issuer, "test-issuer")
            .With(x => x.Audience, "test-audience")
            .Create();

        // Act
        FluentValidation.Results.ValidationResult result = _validator.Validate(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be(Errors.Configuration.JwtSecretKeyTooShort.Description);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-60)]
    public void JwtSettingsModelValidator_WhenExpiryMinutesNotPositive_ShouldHaveValidationError(int minutes)
    {
        // Arrange
        JwtSettingsModel model = _fixture.Build<JwtSettingsModel>()
            .With(x => x.SecretKey, "this-is-a-very-long-secret-key-for-testing")
            .With(x => x.ExpiryMinutes, minutes)
            .With(x => x.Issuer, "test-issuer")
            .With(x => x.Audience, "test-audience")
            .Create();

        // Act
        FluentValidation.Results.ValidationResult result = _validator.Validate(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be(Errors.Configuration.JwtExpiryMinutesMustBePositive.Description);
    }

    [Fact]
    public void JwtSettingsModelValidator_WhenIssuerIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        JwtSettingsModel model = _fixture.Build<JwtSettingsModel>()
            .With(x => x.SecretKey, "this-is-a-very-long-secret-key-for-testing")
            .With(x => x.ExpiryMinutes, 30)
            .With(x => x.Issuer, string.Empty)
            .With(x => x.Audience, "test-audience")
            .Create();

        // Act
        FluentValidation.Results.ValidationResult result = _validator.Validate(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be(Errors.Configuration.JwtIssuerCannotBeEmpty.Description);
    }

    [Fact]
    public void JwtSettingsModelValidator_WhenAudienceIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        JwtSettingsModel model = _fixture.Build<JwtSettingsModel>()
            .With(x => x.SecretKey, "this-is-a-very-long-secret-key-for-testing")
            .With(x => x.ExpiryMinutes, 30)
            .With(x => x.Issuer, "test-issuer")
            .With(x => x.Audience, string.Empty)
            .Create();

        // Act
        FluentValidation.Results.ValidationResult result = _validator.Validate(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be(Errors.Configuration.JwtAudienceCannotBeEmpty.Description);
    }
}
