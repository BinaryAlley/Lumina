#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Scheduling.Commands.AddScheduledJob;
using Lumina.Application.Fixtures.Core.Scheduling.Commands.AddScheduledJob;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Scheduling;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.Scheduling.Commands.AddScheduledJob;

/// <summary>
/// Contains unit tests for the <see cref="AddScheduledJobCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddScheduledJobCommandValidatorTests
{
    private readonly AddScheduledJobCommandValidator _validator = new();
    private readonly AddScheduledJobCommandFixture _addScheduledJobCommandFixture = new();

    [Theory]
    [InlineData(null)] // null name
    [InlineData("")] // empty name
    [InlineData("   ")] // whitespace name
    public void Validate_WhenNameIsNullOrWhitespace_ShouldHaveValidationError(string? name)
    {
        // Arrange
        AddScheduledJobCommand command = _addScheduledJobCommandFixture.Create() with { Name = name! };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Scheduling.ScheduledJobNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenNameExceedsMaximumLength_ShouldHaveValidationError()
    {
        // Arrange
        AddScheduledJobCommand command = _addScheduledJobCommandFixture.Create(name: new string('a', 81));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Scheduling.ScheduledJobNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTaskTypeIsNotInEnum_ShouldHaveValidationError()
    {
        // Arrange
        AddScheduledJobCommand command = _addScheduledJobCommandFixture.Create(taskType: (ScheduledTaskType)999);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Scheduling.InvalidScheduleType);
    }

    [Fact]
    public void Validate_WhenScheduleTypeIsNotInEnum_ShouldHaveValidationError()
    {
        // Arrange
        AddScheduledJobCommand command = _addScheduledJobCommandFixture.Create(scheduleType: (ScheduleType)999);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Scheduling.InvalidScheduleType);
    }

    [Fact]
    public void Validate_WhenIntervalScheduleHasNullInterval_ShouldHaveValidationError()
    {
        // Arrange
        AddScheduledJobCommand command = _addScheduledJobCommandFixture.Create(scheduleType: ScheduleType.WithIntervalInMinutes) with { IntervalMinutes = null };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Scheduling.IntervalMinutesMustBePositive);
    }

    [Theory]
    [InlineData(0)] // interval of zero
    [InlineData(-1)] // negative interval
    public void Validate_WhenIntervalScheduleHasNonPositiveInterval_ShouldHaveValidationError(int intervalMinutes)
    {
        // Arrange
        AddScheduledJobCommand command = _addScheduledJobCommandFixture.Create(
            scheduleType: ScheduleType.WithIntervalInMinutes,
            intervalMinutes: intervalMinutes);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Scheduling.IntervalMinutesMustBePositive);
    }

    [Fact]
    public void Validate_WhenDailyScheduleHasNullHour_ShouldHaveValidationError()
    {
        // Arrange
        AddScheduledJobCommand command = _addScheduledJobCommandFixture.Create(scheduleType: ScheduleType.DailyAtHourAndMinute) with { Hour = null };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Scheduling.HourMustBeBetweenZeroAndTwentyThree);
    }

    [Fact]
    public void Validate_WhenDailyScheduleHasNullMinute_ShouldHaveValidationError()
    {
        // Arrange
        AddScheduledJobCommand command = _addScheduledJobCommandFixture.Create(scheduleType: ScheduleType.DailyAtHourAndMinute) with { Minute = null };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Scheduling.MinuteMustBeBetweenZeroAndFiftyNine);
    }

    [Theory]
    [InlineData(-1)] // hour below the allowed range
    [InlineData(24)] // hour above the allowed range
    public void Validate_WhenDailyScheduleHasOutOfRangeHour_ShouldHaveValidationError(int hour)
    {
        // Arrange
        AddScheduledJobCommand command = _addScheduledJobCommandFixture.Create(
            scheduleType: ScheduleType.DailyAtHourAndMinute,
            hour: hour,
            minute: 0);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Scheduling.HourMustBeBetweenZeroAndTwentyThree);
    }

    [Theory]
    [InlineData(-1)] // minute below the allowed range
    [InlineData(60)] // minute above the allowed range
    public void Validate_WhenDailyScheduleHasOutOfRangeMinute_ShouldHaveValidationError(int minute)
    {
        // Arrange
        AddScheduledJobCommand command = _addScheduledJobCommandFixture.Create(
            scheduleType: ScheduleType.DailyAtHourAndMinute,
            hour: 12,
            minute: minute);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Scheduling.MinuteMustBeBetweenZeroAndFiftyNine);
    }

    [Fact]
    public void Validate_WhenCommandIsValidWithIntervalSchedule_ShouldNotHaveAnyValidationError()
    {
        // Arrange
        AddScheduledJobCommand command = _addScheduledJobCommandFixture.Create(scheduleType: ScheduleType.WithIntervalInMinutes);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenCommandIsValidWithDailySchedule_ShouldNotHaveAnyValidationError()
    {
        // Arrange
        AddScheduledJobCommand command = _addScheduledJobCommandFixture.Create(scheduleType: ScheduleType.DailyAtHourAndMinute);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
