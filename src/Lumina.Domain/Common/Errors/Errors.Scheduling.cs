#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
#endregion

namespace Lumina.Domain.Common.Errors;

/// <summary>
/// Scheduled job error types.
/// </summary>
public static partial class Errors
{
    /// <summary>
    /// Groups the errors that report scheduled job rule violations and scheduling conflicts.
    /// </summary>
    public static class Scheduling
    {
        /// <summary>
        /// Gets the error that reports that the scheduled job name cannot be empty.
        /// </summary>
        public static Error ScheduledJobNameCannotBeEmpty => Error.Validation(description: nameof(ScheduledJobNameCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that the interval in minutes must be a positive number.
        /// </summary>
        public static Error IntervalMinutesMustBePositive => Error.Validation(description: nameof(IntervalMinutesMustBePositive));

        /// <summary>
        /// Gets the error that reports that the hour must be between 0 and 23.
        /// </summary>
        public static Error HourMustBeBetweenZeroAndTwentyThree => Error.Validation(description: nameof(HourMustBeBetweenZeroAndTwentyThree));

        /// <summary>
        /// Gets the error that reports that the minute must be between 0 and 59.
        /// </summary>
        public static Error MinuteMustBeBetweenZeroAndFiftyNine => Error.Validation(description: nameof(MinuteMustBeBetweenZeroAndFiftyNine));

        /// <summary>
        /// Gets the error that reports that the scheduled job already exists.
        /// </summary>
        public static Error ScheduledJobAlreadyExists => Error.Conflict(description: nameof(ScheduledJobAlreadyExists));

        /// <summary>
        /// Gets the error that reports that the requested scheduled job was not found.
        /// </summary>
        public static Error ScheduledJobNotFound => Error.NotFound(description: nameof(ScheduledJobNotFound));

        /// <summary>
        /// Gets the error that reports that the scheduled job execution already exists.
        /// </summary>
        public static Error ScheduledJobExecutionAlreadyExists => Error.Conflict(description: nameof(ScheduledJobExecutionAlreadyExists));

        /// <summary>
        /// Gets the error that reports that the requested scheduled job execution was not found.
        /// </summary>
        public static Error ScheduledJobExecutionNotFound => Error.NotFound(description: nameof(ScheduledJobExecutionNotFound));

        /// <summary>
        /// Gets the error that reports that the cycle of the scheduled job was already started.
        /// </summary>
        public static Error ScheduledJobCycleAlreadyStarted => Error.Forbidden(description: nameof(ScheduledJobCycleAlreadyStarted));

        /// <summary>
        /// Gets the error that reports that the cycle of the scheduled job was not started.
        /// </summary>
        public static Error ScheduledJobNotStarted => Error.Forbidden(description: nameof(ScheduledJobNotStarted));

        /// <summary>
        /// Gets the error that reports that the scheduled job is already running.
        /// </summary>
        public static Error ScheduledJobAlreadyRunning => Error.Forbidden(description: nameof(ScheduledJobAlreadyRunning));

        /// <summary>
        /// Gets the error that reports that only a running scheduled job can be completed.
        /// </summary>
        public static Error CanOnlyCompleteRunningScheduledJob => Error.Forbidden(description: nameof(CanOnlyCompleteRunningScheduledJob));

        /// <summary>
        /// Gets the error that reports that the schedule type is not a defined value.
        /// </summary>
        public static Error InvalidScheduleType => Error.Validation(description: nameof(InvalidScheduleType));
    }
}
