#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
#endregion

namespace Lumina.Application.Common.Errors;

/// <summary>
/// Application scheduling related error types.
/// </summary>
public static partial class Errors
{
    /// <summary>
    /// Error types for application scheduling operations.
    /// </summary>
    public static class Scheduling
    {
        /// <summary>
        /// Gets the error that reports that the scheduler display time span must be a positive value.
        /// </summary>
        public static Error SchedulerDisplayTimeSpanMustBePositive => Error.Validation(description: nameof(SchedulerDisplayTimeSpanMustBePositive));
    }
}
