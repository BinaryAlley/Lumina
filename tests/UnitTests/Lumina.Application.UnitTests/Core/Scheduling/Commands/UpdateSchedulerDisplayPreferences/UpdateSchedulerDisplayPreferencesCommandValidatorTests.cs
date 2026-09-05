#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Scheduling.Commands.UpdateSchedulerDisplayPreferences;
using Lumina.Application.Fixtures.Core.Scheduling.Commands.UpdateSchedulerDisplayPreferences;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.Scheduling.Commands.UpdateSchedulerDisplayPreferences;

/// <summary>
/// Contains unit tests for the <see cref="UpdateSchedulerDisplayPreferencesCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateSchedulerDisplayPreferencesCommandValidatorTests
{
    private readonly UpdateSchedulerDisplayPreferencesCommandValidator _validator = new();
    private readonly UpdateSchedulerDisplayPreferencesCommandFixture _updateSchedulerDisplayPreferencesCommandFixture = new();

    [Theory]
    [InlineData(0)] // display time span of zero
    [InlineData(-1)] // negative display time span
    [InlineData(-100)] // negative display time span far from zero
    public void Validate_WhenDisplayTimeSpanIsNotPositive_ShouldHaveValidationError(int displayTimeSpan)
    {
        // Arrange
        UpdateSchedulerDisplayPreferencesCommand command = _updateSchedulerDisplayPreferencesCommandFixture.Create(displayTimeSpan: displayTimeSpan);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Scheduling.SchedulerDisplayTimeSpanMustBePositive);
    }

    [Theory]
    [InlineData(1)] // minimal positive display time span
    [InlineData(10)] // default display time span
    [InlineData(1440)] // a day expressed in minutes
    public void Validate_WhenDisplayTimeSpanIsPositive_ShouldNotHaveValidationError(int displayTimeSpan)
    {
        // Arrange
        UpdateSchedulerDisplayPreferencesCommand command = _updateSchedulerDisplayPreferencesCommandFixture.Create(displayTimeSpan: displayTimeSpan);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Scheduling.SchedulerDisplayTimeSpanMustBePositive);
    }

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveAnyValidationError()
    {
        // Arrange
        UpdateSchedulerDisplayPreferencesCommand command = _updateSchedulerDisplayPreferencesCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
