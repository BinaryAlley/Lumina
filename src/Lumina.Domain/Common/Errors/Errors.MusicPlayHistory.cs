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
        public static Error PlayIsNotInProgress => Error.Conflict(description: nameof(PlayIsNotInProgress));
        public static Error ListenedDurationCannotDecrease => Error.Validation(description: nameof(ListenedDurationCannotDecrease));
        public static Error InProgressPlayCannotHaveEndTime => Error.Validation(description: nameof(InProgressPlayCannotHaveEndTime));
        public static Error EndedPlayMustHaveEndTime => Error.Validation(description: nameof(EndedPlayMustHaveEndTime));
    }
}
