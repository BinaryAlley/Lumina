#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Enums.Scheduling;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Scheduling;

/// <summary>
/// Data transfer object for the display preferences of the scheduler page.
/// </summary>
/// <param name="UserId">The unique identifier of the user that owns the display preferences.</param>
/// <param name="JobTypeFilter">The type of the scheduled job tasks whose executions are shown on the scheduler page, or <see langword="null"/> when all of them are shown.</param>
/// <param name="DisplayTimeSpan">The time span, expressed in <paramref name="DisplayTimeUnit"/>, that the scheduler page shows.</param>
/// <param name="DisplayTimeUnit">The unit in which the displayed time span of the scheduler page is expressed.</param>
[DebuggerDisplay("UserId: {UserId}, DisplayTimeSpan: {DisplayTimeSpan} {DisplayTimeUnit}")]
public record SchedulerDisplayPreferencesDto(
    Guid UserId,
    ScheduledTaskType? JobTypeFilter,
    int DisplayTimeSpan,
    SchedulerDisplayTimeUnit DisplayTimeUnit
);
