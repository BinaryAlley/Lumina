#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
#endregion

namespace Lumina.Domain.Common.Errors;

/// <summary>
/// Music play history error types.
/// </summary>
public static partial class Errors
{
    public static class MusicPlayHistory
    {
        public static Error MusicPlayRecordNotFound => Error.NotFound(description: nameof(MusicPlayRecordNotFound));
        public static Error MusicPlayRecordAlreadyExists => Error.Conflict(description: nameof(MusicPlayRecordAlreadyExists));
        public static Error PlayRecordIdCannotBeEmpty => Error.Validation(description: nameof(PlayRecordIdCannotBeEmpty));
        public static Error PlaybackPositionCannotBeNull => Error.Validation(description: nameof(PlaybackPositionCannotBeNull));
        public static Error PlaybackPositionMustBeNonNegative => Error.Validation(description: nameof(PlaybackPositionMustBeNonNegative));
        public static Error PlaybackPositionIsOutOfRange => Error.Validation(description: nameof(PlaybackPositionIsOutOfRange));
        public static Error ListenedDurationCannotBeNull => Error.Validation(description: nameof(ListenedDurationCannotBeNull));
        public static Error ListenedDurationMustBeNonNegative => Error.Validation(description: nameof(ListenedDurationMustBeNonNegative));
        public static Error ListenedDurationIsOutOfRange => Error.Validation(description: nameof(ListenedDurationIsOutOfRange));
        public static Error PlayIsNotInProgress => Error.Conflict(description: nameof(PlayIsNotInProgress));
        public static Error ListenedDurationCannotDecrease => Error.Validation(description: nameof(ListenedDurationCannotDecrease));
        public static Error InProgressPlayCannotHaveEndTime => Error.Validation(description: nameof(InProgressPlayCannotHaveEndTime));
        public static Error EndedPlayMustHaveEndTime => Error.Validation(description: nameof(EndedPlayMustHaveEndTime));
    }
}
