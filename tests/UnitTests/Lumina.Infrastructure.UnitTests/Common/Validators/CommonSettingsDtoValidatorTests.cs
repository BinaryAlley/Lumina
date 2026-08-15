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
/// Contains unit tests for the <see cref="CommonSettingsDtoValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CommonSettingsDtoValidatorTests
{
    private readonly CommonSettingsDtoValidator _validator = new();
    private readonly IFixture _fixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommonSettingsDtoValidatorTests"/> class.
    /// </summary>
    public CommonSettingsDtoValidatorTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
    }

    [Fact]
    public void CommonSettingsModelValidator_WhenThemeProvided_ShouldNotHaveValidationError()
    {
        // Arrange
        CommonSettingsDto model = _fixture.Build<CommonSettingsDto>()
            .With(x => x.Theme, "Dark")
            .Create();

        // Act
        List<Error> result = _validator.Validate(model);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void CommonSettingsModelValidator_WhenThemeNotProvided_ShouldNotHaveValidationError()
    {
        // Arrange
        CommonSettingsDto model = _fixture.Build<CommonSettingsDto>()
            .With(x => x.Theme, string.Empty)
            .Create();

        // Act
        List<Error> result = _validator.Validate(model);

        // Assert
        Assert.Single(result);
        Assert.Equal(Errors.Configuration.ApplicationThemeCannotBeEmpty.Description, result[0].Description);
    }
}
