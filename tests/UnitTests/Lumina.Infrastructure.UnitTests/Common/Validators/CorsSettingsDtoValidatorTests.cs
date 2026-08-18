#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Infrastructure.Common.Errors;
using Lumina.Infrastructure.Common.Models.DTO.Configuration;
using Lumina.Infrastructure.Common.Validators;
using Lumina.Infrastructure.Fixtures.Common.Models.DTO.Configuration;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Infrastructure.UnitTests.Common.Validators;

/// <summary>
/// Contains unit tests for the <see cref="CorsSettingsDtoValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CorsSettingsDtoValidatorTests
{
    private readonly CorsSettingsDtoValidator _validator = new();
    private readonly CorsSettingsDtoFixture _corsSettingsDtoFixture = new();

    [Fact]
    public void Validate_WhenOriginsAreValidAbsoluteUrls_ShouldNotHaveValidationError()
    {
        // Arrange
        CorsSettingsDto model = _corsSettingsDtoFixture.Create(["https://app.example.com", "http://localhost:4200"]);

        // Act
        List<Error> result = _validator.Validate(model);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_WhenOriginIsWildcard_ShouldNotHaveValidationError()
    {
        // Arrange
        CorsSettingsDto model = _corsSettingsDtoFixture.Create(["*"]);

        // Act
        List<Error> result = _validator.Validate(model);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_WhenAllowedOriginsIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        CorsSettingsDto model = _corsSettingsDtoFixture.Create([]);

        // Act
        List<Error> result = _validator.Validate(model);

        // Assert
        Assert.Single(result);
        Assert.Equal(Errors.Configuration.CorsOriginsCannotBeEmpty.Description, result[0].Description);
    }

    [Fact]
    public void Validate_WhenOriginHasTrailingSlash_ShouldHaveValidationError()
    {
        // Arrange
        CorsSettingsDto model = _corsSettingsDtoFixture.Create(["https://app.example.com/"]);

        // Act
        List<Error> result = _validator.Validate(model);

        // Assert
        Assert.Single(result);
        Assert.Equal(Errors.Configuration.CorsOriginIsInvalid.Description, result[0].Description);
    }

    [Fact]
    public void Validate_WhenOriginHasNoScheme_ShouldHaveValidationError()
    {
        // Arrange
        CorsSettingsDto model = _corsSettingsDtoFixture.Create(["app.example.com"]);

        // Act
        List<Error> result = _validator.Validate(model);

        // Assert
        Assert.Single(result);
        Assert.Equal(Errors.Configuration.CorsOriginIsInvalid.Description, result[0].Description);
    }

    [Fact]
    public void Validate_WhenOriginHasUnsupportedScheme_ShouldHaveValidationError()
    {
        // Arrange
        CorsSettingsDto model = _corsSettingsDtoFixture.Create(["ftp://app.example.com"]);

        // Act
        List<Error> result = _validator.Validate(model);

        // Assert
        Assert.Single(result);
        Assert.Equal(Errors.Configuration.CorsOriginIsInvalid.Description, result[0].Description);
    }

    [Fact]
    public void Validate_WhenOneOriginIsInvalid_ShouldHaveValidationErrorForThatOrigin()
    {
        // Arrange
        CorsSettingsDto model = _corsSettingsDtoFixture.Create(["https://valid.example.com", "invalid-origin"]);

        // Act
        List<Error> result = _validator.Validate(model);

        // Assert
        Assert.Single(result);
        Assert.Equal(Errors.Configuration.CorsOriginIsInvalid.Description, result[0].Description);
    }

    [Fact]
    public void Validate_WhenOriginIsEmptyString_ShouldHaveValidationError()
    {
        // Arrange
        CorsSettingsDto model = _corsSettingsDtoFixture.Create([""]);

        // Act
        List<Error> result = _validator.Validate(model);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, error => error.Description == Errors.Configuration.CorsOriginsCannotBeEmpty.Description);
        Assert.Contains(result, error => error.Description == Errors.Configuration.CorsOriginIsInvalid.Description);
    }
}
