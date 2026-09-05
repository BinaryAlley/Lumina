#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.SharedKernel.Common.Enums.Scheduling;
#endregion

namespace Lumina.Application.Core.Scheduling.Commands.AddScheduledJob;

/// <summary>
/// Validates the needed validation rules for <see cref="AddScheduledJobCommand"/>.
/// </summary>
public class AddScheduledJobCommandValidator : AbstractValidator<AddScheduledJobCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddScheduledJobCommandValidator"/> class.
    /// </summary>
    public AddScheduledJobCommandValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty()
            .WithError(Errors.Scheduling.ScheduledJobNameCannotBeEmpty)
            .MaximumLength(80)
            .WithError(Errors.Scheduling.ScheduledJobNameCannotBeEmpty);

        RuleFor(command => command.TaskType)
            .IsInEnum()
            .WithError(Errors.Scheduling.InvalidScheduleType);

        RuleFor(command => command.ScheduleType)
            .IsInEnum()
            .WithError(Errors.Scheduling.InvalidScheduleType);

        RuleFor(command => command.IntervalMinutes)
            .Must(intervalMinutes => intervalMinutes is null || intervalMinutes > 0)
            .WithError(Errors.Scheduling.IntervalMinutesMustBePositive)
            .When(command => command.ScheduleType == ScheduleType.WithIntervalInMinutes);

        RuleFor(command => command.IntervalMinutes)
            .Must(intervalMinutes => intervalMinutes is not null)
            .WithError(Errors.Scheduling.IntervalMinutesMustBePositive)
            .When(command => command.ScheduleType == ScheduleType.WithIntervalInMinutes);

        RuleFor(command => command.Hour)
            .Must(hour => hour is not null)
            .WithError(Errors.Scheduling.HourMustBeBetweenZeroAndTwentyThree)
            .When(command => command.ScheduleType == ScheduleType.DailyAtHourAndMinute);

        RuleFor(command => command.Minute)
            .Must(minute => minute is not null)
            .WithError(Errors.Scheduling.MinuteMustBeBetweenZeroAndFiftyNine)
            .When(command => command.ScheduleType == ScheduleType.DailyAtHourAndMinute);

        RuleFor(command => command.Hour)
            .InclusiveBetween(0, 23)
            .WithError(Errors.Scheduling.HourMustBeBetweenZeroAndTwentyThree)
            .When(command => command.ScheduleType == ScheduleType.DailyAtHourAndMinute);

        RuleFor(command => command.Minute)
            .InclusiveBetween(0, 59)
            .WithError(Errors.Scheduling.MinuteMustBeBetweenZeroAndFiftyNine)
            .When(command => command.ScheduleType == ScheduleType.DailyAtHourAndMinute);
    }
}
