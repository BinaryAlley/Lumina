#region ========================================================================= USING =====================================================================================
using Nemera.Application.Core.Scheduling.Commands.UpdateSchedulerDisplayPreferences;
using Nemera.Application.Fixtures.Core.Scheduling.Commands.UpdateSchedulerDisplayPreferences;
using Nemera.Application.Common.Errors;
using Nemera.Application.UnitTests.Common.Setup;
using Nemera.Domain.Common.Primitives;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Application.UnitTests.Core.Scheduling.Commands.UpdateSchedulerDisplayPreferences;

/// <summary>
/// Contains unit tests for the <see cref="UpdateSchedulerDisplayPreferencesCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateSchedulerDisplayPreferencesCommandValidatorTests
{
    private readonly UpdateSchedulerDisplayPreferencesCommandValidator _validator = new();
    private readonly UpdateSchedulerDisplayPreferencesCommandFixture _updateSchedulerDisplayPreferencesCommandFixture = new();

    [Theory]
    [InlineData(0)] // Display time span of zero.
    [InlineData(-1)] // Negative display time span.
    [InlineData(-100)] // Negative display time span far from zero.
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
    [InlineData(1)] // Minimal positive display time span.
    [InlineData(10)] // Default display time span.
    [InlineData(1440)] // A day expressed in minutes.
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
