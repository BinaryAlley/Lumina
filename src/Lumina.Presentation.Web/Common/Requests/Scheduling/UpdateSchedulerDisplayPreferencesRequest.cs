#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Enums.Scheduling;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.Scheduling;

/// <summary>
/// Request for updating the display preferences of the scheduler page of the current user.
/// </summary>
/// <param name="JobTypeFilter">The type of the scheduled job tasks whose executions are shown on the scheduler page, or <see langword="null"/> when all of them are shown.</param>
/// <param name="DisplayTimeSpan">The time span, expressed in <paramref name="DisplayTimeUnit"/>, that the scheduler page shows. Required.</param>
/// <param name="DisplayTimeUnit">The unit in which the displayed time span of the scheduler page is expressed. Required.</param>
[DebuggerDisplay("DisplayTimeSpan: {DisplayTimeSpan} {DisplayTimeUnit}")]
public record UpdateSchedulerDisplayPreferencesRequest(
    ScheduledTaskType? JobTypeFilter,
    int DisplayTimeSpan,
    SchedulerDisplayTimeUnit DisplayTimeUnit
);
