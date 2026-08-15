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
public class DatabaseSettingsDtoValidatorTests
{
    private readonly DatabaseSettingsDtoValidator _validator = new();
    private readonly IFixture _fixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommonSettingsDtoValidatorTests"/> class.
    /// </summary>
    public DatabaseSettingsDtoValidatorTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
    }

    [Fact]
    public void DatabaseSettingsModelValidator_WhenDefaultConnectionProvided_ShouldNotHaveValidationError()
    {
        // Arrange
        DatabaseSettingsDto model = _fixture.Build<DatabaseSettingsDto>()
            .With(x => x.DefaultConnection, "dummy-connection-string")
            .Create();

        // Act
        List<Error> result = _validator.Validate(model);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void DatabaseSettingsModelValidator_WhenDefaultConnectionNotProvided_ShouldHaveValidationError()
    {
        // Arrange
        DatabaseSettingsDto model = _fixture.Build<DatabaseSettingsDto>()
            .With(x => x.DefaultConnection, string.Empty)
            .Create();

        // Act
        List<Error> result = _validator.Validate(model);

        // Assert
        Assert.Single(result);
        Assert.Equal(Errors.Configuration.DatabaseConnectionStringCannotBeEmpty.Description, result[0].Description);
    }
}
