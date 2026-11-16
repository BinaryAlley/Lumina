#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
#endregion

namespace Lumina.Domain.Common.Errors;

/// <summary>
/// Media consumption error types.
/// </summary>
public static partial class Errors
{
    /// <summary>
    /// Groups the errors that report media consumption rule violations and missing consumption records.
    /// </summary>
    public static class MediaConsumption
    {
        /// <summary>
        /// Gets the error that reports that the media kind does not support consumption tracking.
        /// </summary>
        public static Error UnsupportedMediaKind => Error.Validation(description: nameof(UnsupportedMediaKind));

        /// <summary>
        /// Gets the error that reports that the media kind cannot start in a completed state because it is not a one-shot kind.
        /// </summary>
        public static Error MediaKindCannotStartCompleted => Error.Forbidden(description: nameof(MediaKindCannotStartCompleted));

        /// <summary>
        /// Gets the error that reports that a consumption of the media kind cannot be resumed.
        /// </summary>
        public static Error MediaKindCannotResume => Error.Forbidden(description: nameof(MediaKindCannotResume));

        /// <summary>
        /// Gets the error that reports that the requested consumption record was not found.
        /// </summary>
        public static Error ConsumptionRecordNotFound => Error.NotFound(description: nameof(ConsumptionRecordNotFound));

        /// <summary>
        /// Gets the error that reports that the resume point of the media item was not found.
        /// </summary>
        public static Error MediaResumePointNotFound => Error.NotFound(description: nameof(MediaResumePointNotFound));

        /// <summary>
        /// Gets the error that reports that the consumption record identifier cannot be empty.
        /// </summary>
        public static Error ConsumptionRecordIdCannotBeEmpty => Error.Validation(description: nameof(ConsumptionRecordIdCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that the media item identifier cannot be empty.
        /// </summary>
        public static Error MediaItemIdCannotBeEmpty => Error.Validation(description: nameof(MediaItemIdCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that the target user identifier is not valid.
        /// </summary>
        public static Error TargetUserIdIsInvalid => Error.Validation(description: nameof(TargetUserIdIsInvalid));

        /// <summary>
        /// Gets the error that reports that the consumption date range is not valid.
        /// </summary>
        public static Error ConsumptionDateRangeIsInvalid => Error.Validation(description: nameof(ConsumptionDateRangeIsInvalid));

        /// <summary>
        /// Gets the error that reports that the media position is not valid.
        /// </summary>
        public static Error InvalidMediaPosition => Error.Validation(description: nameof(InvalidMediaPosition));

        /// <summary>
        /// Gets the error that reports that the progress fraction is not valid.
        /// </summary>
        public static Error InvalidProgressFraction => Error.Validation(description: nameof(InvalidProgressFraction));

        /// <summary>
        /// Gets the error that reports that the consumed duration cannot be negative.
        /// </summary>
        public static Error ConsumedDurationMustBeNonNegative => Error.Validation(description: nameof(ConsumedDurationMustBeNonNegative));

        /// <summary>
        /// Gets the error that reports that the consumed duration is outside the allowed range.
        /// </summary>
        public static Error ConsumedDurationIsOutOfRange => Error.Validation(description: nameof(ConsumedDurationIsOutOfRange));

        /// <summary>
        /// Gets the error that reports that the consumed duration cannot decrease.
        /// </summary>
        public static Error ConsumedDurationCannotDecrease => Error.Forbidden(description: nameof(ConsumedDurationCannotDecrease));

        /// <summary>
        /// Gets the error that reports that the consecutive pages read count cannot be negative.
        /// </summary>
        public static Error ConsecutivePagesReadMustBeNonNegative => Error.Validation(description: nameof(ConsecutivePagesReadMustBeNonNegative));

        /// <summary>
        /// Gets the error that reports that consecutive pages read is only supported for page-based media.
        /// </summary>
        public static Error ConsecutivePagesReadIsOnlySupportedForPageBasedMedia => Error.Forbidden(description: nameof(ConsecutivePagesReadIsOnlySupportedForPageBasedMedia));

        /// <summary>
        /// Gets the error that reports that the consumption is not in progress.
        /// </summary>
        public static Error ConsumptionIsNotInProgress => Error.Conflict(description: nameof(ConsumptionIsNotInProgress));

        /// <summary>
        /// Gets the error that reports that the consumption already reached a terminal state.
        /// </summary>
        public static Error ConsumptionIsAlreadyTerminal => Error.Conflict(description: nameof(ConsumptionIsAlreadyTerminal));

        /// <summary>
        /// Gets the error that reports that a consumption in progress cannot have an end time.
        /// </summary>
        public static Error InProgressConsumptionCannotHaveEndTime => Error.Validation(description: nameof(InProgressConsumptionCannotHaveEndTime));

        /// <summary>
        /// Gets the error that reports that an ended consumption must have an end time.
        /// </summary>
        public static Error EndedConsumptionMustHaveEndTime => Error.Validation(description: nameof(EndedConsumptionMustHaveEndTime));

        /// <summary>
        /// Gets the error that reports that the last activity of a consumption cannot precede its start.
        /// </summary>
        public static Error LastActivityCannotPrecedeStart => Error.Forbidden(description: nameof(LastActivityCannotPrecedeStart));

        /// <summary>
        /// Gets the error that reports that the end of a consumption cannot precede its last activity.
        /// </summary>
        public static Error EndedConsumptionCannotPrecedeLastActivity => Error.Forbidden(description: nameof(EndedConsumptionCannotPrecedeLastActivity));
    }
}
