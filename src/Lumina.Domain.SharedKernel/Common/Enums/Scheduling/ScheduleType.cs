namespace Lumina.Domain.SharedKernel.Common.Enums.Scheduling;

/// <summary>
/// Enumeration for the types of schedules of scheduled jobs.
/// </summary>
public enum ScheduleType
{
    /// <summary>
    /// The scheduled job repeats with an interval measured in minutes.
    /// </summary>
    WithIntervalInMinutes,

    /// <summary>
    /// The scheduled job repeats daily at a fixed hour and minute.
    /// </summary>
    DailyAtHourAndMinute,

    /// <summary>
    /// The scheduled job fires its task once, every time the application starts.
    /// </summary>
    OnceAtStartup
}
