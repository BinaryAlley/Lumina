#region ========================================================================= USING =====================================================================================
using System;
#endregion

namespace Lumina.Application.Common.Utilities;

/// <summary>
/// Extension methods for converting stored duration values to <see cref="TimeSpan"/>.
/// </summary>
public static class DurationUtilities
{
    /// <summary>
    /// Converts a duration stored as whole milliseconds to a <see cref="TimeSpan"/>, clamped to the largest value a <see cref="TimeSpan"/> can
    /// represent so that an accumulated total above that limit does not throw.
    /// </summary>
    /// <param name="milliseconds">The duration, in milliseconds.</param>
    /// <returns>The duration, clamped to <see cref="TimeSpan.MaxValue"/> when it exceeds the representable range.</returns>
    public static TimeSpan ToClampedTimeSpan(this long milliseconds)
    {
        long maximumMilliseconds = TimeSpan.MaxValue.Ticks / TimeSpan.TicksPerMillisecond;
        return TimeSpan.FromMilliseconds(Math.Min(milliseconds, maximumMilliseconds));
    }
}
