#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
#endregion

namespace Lumina.Domain.Common.Errors;

/// <summary>
/// Media consumption error types.
/// </summary>
public static partial class Errors
{
    public static class MediaConsumption
    {
        public static Error UnsupportedMediaKind => Error.Validation(description: nameof(UnsupportedMediaKind));
        public static Error MediaKindCannotStartCompleted => Error.Forbidden(description: nameof(MediaKindCannotStartCompleted));
        public static Error MediaKindCannotResume => Error.Forbidden(description: nameof(MediaKindCannotResume));
        public static Error InvalidMediaPosition => Error.Validation(description: nameof(InvalidMediaPosition));
        public static Error InvalidProgressFraction => Error.Validation(description: nameof(InvalidProgressFraction));
        public static Error ConsumedDurationMustBeNonNegative => Error.Validation(description: nameof(ConsumedDurationMustBeNonNegative));
        public static Error ConsumedDurationCannotDecrease => Error.Forbidden(description: nameof(ConsumedDurationCannotDecrease));
        public static Error ConsecutivePagesReadMustBeNonNegative => Error.Validation(description: nameof(ConsecutivePagesReadMustBeNonNegative));
        public static Error ConsecutivePagesReadIsOnlySupportedForPageBasedMedia => Error.Forbidden(description: nameof(ConsecutivePagesReadIsOnlySupportedForPageBasedMedia));
        public static Error ConsumptionIsNotInProgress => Error.Conflict(description: nameof(ConsumptionIsNotInProgress));
        public static Error ConsumptionIsAlreadyTerminal => Error.Conflict(description: nameof(ConsumptionIsAlreadyTerminal));
        public static Error InProgressConsumptionCannotHaveEndTime => Error.Validation(description: nameof(InProgressConsumptionCannotHaveEndTime));
        public static Error EndedConsumptionMustHaveEndTime => Error.Validation(description: nameof(EndedConsumptionMustHaveEndTime));
        public static Error LastActivityCannotPrecedeStart => Error.Forbidden(description: nameof(LastActivityCannotPrecedeStart));
        public static Error EndedConsumptionCannotPrecedeLastActivity => Error.Forbidden(description: nameof(EndedConsumptionCannotPrecedeLastActivity));
    }
}
