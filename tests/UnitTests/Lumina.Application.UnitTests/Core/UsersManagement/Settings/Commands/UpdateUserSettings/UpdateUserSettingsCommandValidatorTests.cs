#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.UsersManagement.Settings.Commands.UpdateUserSettings;
using Lumina.Application.Fixtures.Core.UsersManagement.Settings.Commands.UpdateUserSettings;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.UsersManagement.Settings.Commands.UpdateUserSettings;

/// <summary>
/// Contains unit tests for the <see cref="UpdateUserSettingsCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateUserSettingsCommandValidatorTests
{
    private readonly UpdateUserSettingsCommandValidator _validator = new();
    private readonly UpdateUserSettingsCommandFixture _updateUserSettingsCommandFixture = new();

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_WhenItemsPerPageIsNotPositive_ShouldHaveValidationError(int itemsPerPage)
    {
        // Arrange
        UpdateUserSettingsCommand command = _updateUserSettingsCommandFixture.Create(itemsPerPage: itemsPerPage);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.UserSettings.ItemsPerPageMustBeGreaterThanZero);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(48)]
    [InlineData(1000000)]
    public void Validate_WhenItemsPerPageIsPositive_ShouldNotHaveValidationError(int itemsPerPage)
    {
        // Arrange
        UpdateUserSettingsCommand command = _updateUserSettingsCommandFixture.Create(itemsPerPage: itemsPerPage);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.UserSettings.ItemsPerPageMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveAnyValidationError()
    {
        // Arrange
        UpdateUserSettingsCommand command = _updateUserSettingsCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
