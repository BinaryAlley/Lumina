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
public class DatabaseSettingsModelValidatorTests
{
    private readonly DatabaseSettingsModelValidator _validator;
    private readonly IFixture _fixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommonSettingsModelValidatorTests"/> class.
    /// </summary>
    public DatabaseSettingsModelValidatorTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        _validator = new();
    }

    [Fact]
    public void DatabaseSettingsModelValidator_WhenDefaultConnectionProvided_ShouldNotHaveValidationError()
    {
        // Arrange
        DatabaseSettingsModel model = _fixture.Build<DatabaseSettingsModel>()
            .With(x => x.DefaultConnection, "dummy-connection-string")
            .Create();

        // Act
        FluentValidation.Results.ValidationResult result = _validator.Validate(model);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void DatabaseSettingsModelValidator_WhenDefaultConnectionNotProvided_ShouldHaveValidationError()
    {
        // Arrange
        DatabaseSettingsModel model = _fixture.Build<DatabaseSettingsModel>()
            .With(x => x.DefaultConnection, string.Empty)
            .Create();

        // Act
        FluentValidation.Results.ValidationResult result = _validator.Validate(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be(Errors.Configuration.DatabaseConnectionStringCannotBeEmpty.Description);
    }
}
