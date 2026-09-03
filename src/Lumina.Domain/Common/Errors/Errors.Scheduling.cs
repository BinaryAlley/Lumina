#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
#endregion

namespace Lumina.Domain.Common.Errors;

/// <summary>
/// Scheduled job error types.
/// </summary>
public static partial class Errors
{
    public static class Scheduling
    {
        public static Error ScheduledJobNameCannotBeEmpty => Error.Validation(description: nameof(ScheduledJobNameCannotBeEmpty));
        public static Error IntervalMinutesMustBePositive => Error.Validation(description: nameof(IntervalMinutesMustBePositive));
        public static Error HourMustBeBetweenZeroAndTwentyThree => Error.Validation(description: nameof(HourMustBeBetweenZeroAndTwentyThree));
        public static Error MinuteMustBeBetweenZeroAndFiftyNine => Error.Validation(description: nameof(MinuteMustBeBetweenZeroAndFiftyNine));
        public static Error ScheduledJobAlreadyExists => Error.Conflict(description: nameof(ScheduledJobAlreadyExists));
        public static Error ScheduledJobNotFound => Error.NotFound(description: nameof(ScheduledJobNotFound));
        public static Error ScheduledJobExecutionAlreadyExists => Error.Conflict(description: nameof(ScheduledJobExecutionAlreadyExists));
        public static Error ScheduledJobExecutionNotFound => Error.NotFound(description: nameof(ScheduledJobExecutionNotFound));
        public static Error ScheduledJobCycleAlreadyStarted => Error.Forbidden(description: nameof(ScheduledJobCycleAlreadyStarted));
        public static Error ScheduledJobNotStarted => Error.Forbidden(description: nameof(ScheduledJobNotStarted));
        public static Error ScheduledJobAlreadyRunning => Error.Forbidden(description: nameof(ScheduledJobAlreadyRunning));
        public static Error CanOnlyCompleteRunningScheduledJob => Error.Forbidden(description: nameof(CanOnlyCompleteRunningScheduledJob));
        public static Error InvalidScheduleType => Error.Validation(description: nameof(InvalidScheduleType));
    }
}
