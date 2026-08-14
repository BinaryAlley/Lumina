#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using Lumina.Domain.Common.Primitives;
using Lumina.Infrastructure.Common.Errors;
using Lumina.Infrastructure.Common.Models.DTO.Configuration;
using Lumina.Infrastructure.Common.Validators;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Infrastructure.UnitTests.Common.Validators;

/// <summary>
/// Contains unit tests for the <see cref="JwtSettingsDtoValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class JwtSettingsDtoValidatorTests
{
    private readonly JwtSettingsDtoValidator _validator;
    private readonly IFixture _fixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="JwtSettingsDtoValidatorTests"/> class.
    /// </summary>
    public JwtSettingsDtoValidatorTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        _validator = new();
    }

    [Fact]
    public void JwtSettingsModelValidator_WhenAllPropertiesValid_ShouldNotHaveValidationError()
    {
        // Arrange
        JwtSettingsDto model = _fixture.Build<JwtSettingsDto>()
            .With(x => x.SecretKey, "this-is-a-very-long-secret-key-for-testing")
            .With(x => x.ExpiryMinutes, 30)
            .With(x => x.Issuer, "test-issuer")
            .With(x => x.Audience, "test-audience")
            .Create();

        // Act
        List<Error> result = _validator.Validate(model);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void JwtSettingsModelValidator_WhenSecretKeyIsEmpty_ShouldHaveValidationErrors()
    {
        // Arrange
        JwtSettingsDto model = _fixture.Build<JwtSettingsDto>()
            .With(x => x.SecretKey, string.Empty)
            .With(x => x.ExpiryMinutes, 30)
            .With(x => x.Issuer, "test-issuer")
            .With(x => x.Audience, "test-audience")
            .Create();

        // Act
        List<Error> result = _validator.Validate(model);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, error => error.Description == Errors.Configuration.JwtSecretKeyCannotBeEmpty.Description);
        Assert.Contains(result, error => error.Description == Errors.Configuration.JwtSecretKeyTooShort.Description);
    }

    [Fact]
    public void JwtSettingsModelValidator_WhenSecretKeyTooShort_ShouldHaveValidationError()
    {
        // Arrange
        JwtSettingsDto model = _fixture.Build<JwtSettingsDto>()
            .With(x => x.SecretKey, "short-key")
            .With(x => x.ExpiryMinutes, 30)
            .With(x => x.Issuer, "test-issuer")
            .With(x => x.Audience, "test-audience")
            .Create();

        // Act
        List<Error> result = _validator.Validate(model);

        // Assert
        Assert.Single(result);
        Assert.Equal(Errors.Configuration.JwtSecretKeyTooShort.Description, result[0].Description);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-60)]
    public void JwtSettingsModelValidator_WhenExpiryMinutesNotPositive_ShouldHaveValidationError(int minutes)
    {
        // Arrange
        JwtSettingsDto model = _fixture.Build<JwtSettingsDto>()
            .With(x => x.SecretKey, "this-is-a-very-long-secret-key-for-testing")
            .With(x => x.ExpiryMinutes, minutes)
            .With(x => x.Issuer, "test-issuer")
            .With(x => x.Audience, "test-audience")
            .Create();

        // Act
        List<Error> result = _validator.Validate(model);

        // Assert
        Assert.Single(result);
        Assert.Equal(Errors.Configuration.JwtExpiryMinutesMustBePositive.Description, result[0].Description);
    }

    [Fact]
    public void JwtSettingsModelValidator_WhenIssuerIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        JwtSettingsDto model = _fixture.Build<JwtSettingsDto>()
            .With(x => x.SecretKey, "this-is-a-very-long-secret-key-for-testing")
            .With(x => x.ExpiryMinutes, 30)
            .With(x => x.Issuer, string.Empty)
            .With(x => x.Audience, "test-audience")
            .Create();

        // Act
        List<Error> result = _validator.Validate(model);

        // Assert
        Assert.Single(result);
        Assert.Equal(Errors.Configuration.JwtIssuerCannotBeEmpty.Description, result[0].Description);
    }

    [Fact]
    public void JwtSettingsModelValidator_WhenAudienceIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        JwtSettingsDto model = _fixture.Build<JwtSettingsDto>()
            .With(x => x.SecretKey, "this-is-a-very-long-secret-key-for-testing")
            .With(x => x.ExpiryMinutes, 30)
            .With(x => x.Issuer, "test-issuer")
            .With(x => x.Audience, string.Empty)
            .Create();

        // Act
        List<Error> result = _validator.Validate(model);

        // Assert
        Assert.Single(result);
        Assert.Equal(Errors.Configuration.JwtAudienceCannotBeEmpty.Description, result[0].Description);
    }
}
