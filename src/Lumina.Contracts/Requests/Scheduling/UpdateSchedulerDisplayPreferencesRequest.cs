#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.Scheduling;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.Scheduling;

/// <summary>
/// Represents a request to update the display preferences of the scheduler page of the current user.
/// </summary>
/// <param name="JobTypeFilter">The type of the scheduled job tasks whose executions are shown on the scheduler page, or <see langword="null"/> when all of them are shown.</param>
/// <param name="DisplayTimeSpan">The time span, expressed in <paramref name="DisplayTimeUnit"/>, that the scheduler page shows. Required.</param>
/// <param name="DisplayTimeUnit">The unit in which the displayed time span of the scheduler page is expressed. Required.</param>
[DebuggerDisplay("DisplayTimeSpan: {DisplayTimeSpan} {DisplayTimeUnit}")]
public sealed record UpdateSchedulerDisplayPreferencesRequest(
    ScheduledTaskType? JobTypeFilter,
    int DisplayTimeSpan,
    SchedulerDisplayTimeUnit DisplayTimeUnit
);
