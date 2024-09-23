#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using FluentAssertions;
using Lumina.Infrastructure.Common.Errors;
using Lumina.Infrastructure.Common.Models.Configuration;
using Lumina.Infrastructure.Common.Validators;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Infrastructure.UnitTests.Common.Validators;


/// <summary>
/// Contains unit tests for the <see cref="CommonSettingsModelValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CommonSettingsModelValidatorTests
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly CommonSettingsModelValidator _validator;
    private readonly IFixture _fixture;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="CommonSettingsModelValidatorTests"/> class.
    /// </summary>
    public CommonSettingsModelValidatorTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        _validator = new();
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    [Fact]
    public void CommonSettingsModelValidator_WhenThemeProvided_ShouldNotHaveValidationError()
    {
        // Arrange
        CommonSettingsModel model = _fixture.Build<CommonSettingsModel>()
            .With(x => x.Theme, "Dark")
            .Create();

        // Act
        FluentValidation.Results.ValidationResult result = _validator.Validate(model);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void CommonSettingsModelValidator_WhenThemeNotProvided_ShouldNotHaveValidationError()
    {
        // Arrange
        CommonSettingsModel model = _fixture.Build<CommonSettingsModel>()
            .With(x => x.Theme, string.Empty)
            .Create();

        // Act
        FluentValidation.Results.ValidationResult result = _validator.Validate(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be(Errors.Configuration.ApplicationThemeCannotBeEmpty.Code);
    }
    #endregion
}