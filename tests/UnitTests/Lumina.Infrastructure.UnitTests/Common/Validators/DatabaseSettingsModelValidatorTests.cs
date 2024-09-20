#region ========================================================================= USING =====================================================================================
using System.Diagnostics.CodeAnalysis;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using FluentAssertions;
using Lumina.Infrastructure.Common.Models.Configuration;
using Lumina.Infrastructure.Common.Validators;
using Lumina.Infrastructure.Common.Errors;
#endregion

namespace Lumina.Infrastructure.UnitTests.Common.Validators;

/// <summary>
/// Contains unit tests for the <see cref="CommonSettingsModelValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DatabaseSettingsModelValidatorTests
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly DatabaseSettingsModelValidator _validator;
    private readonly IFixture _fixture;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="CommonSettingsModelValidatorTests"/> class.
    /// </summary>
    public DatabaseSettingsModelValidatorTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        _validator = new();
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    [Fact]
    public void DatabaseSettingsModelValidator_WhenDefaultConnectionProvided_ShouldNotHaveValidationError()
    {
        // Arrange
        var model = _fixture.Build<DatabaseSettingsModel>()
            .With(x => x.DefaultConnection, "dummy-connection-string")
            .Create();

        // Act
        var result = _validator.Validate(model);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void DatabaseSettingsModelValidator_WhenDefaultConnectionNotProvided_ShouldHaveValidationError()
    {
        // Arrange
        var model = _fixture.Build<DatabaseSettingsModel>()
            .With(x => x.DefaultConnection, string.Empty)
            .Create();

        // Act
        var result = _validator.Validate(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be(Errors.Configuration.DatabaseConnectionStringCannotBeEmpty.Code);
    }
    #endregion
}