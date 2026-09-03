namespace Lumina.Presentation.Web.Common.Enums.Scheduling;

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
    DailyAtHourAndMinute
}
