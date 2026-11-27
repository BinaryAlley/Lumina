#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Domain.SharedKernel.Common.Enums.Scheduling;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.Scheduling.Commands.UpdateSchedulerDisplayPreferences;

/// <summary>
/// Command for updating the display preferences of the scheduler page of the current user.
/// </summary>
/// <param name="JobTypeFilter">The type of the scheduled job tasks whose executions are shown on the scheduler page, or <see langword="null"/> when all of them are shown.</param>
/// <param name="DisplayTimeSpan">The time span, expressed in <paramref name="DisplayTimeUnit"/>, that the scheduler page shows.</param>
/// <param name="DisplayTimeUnit">The unit in which the displayed time span of the scheduler page is expressed.</param>
[DebuggerDisplay("DisplayTimeSpan: {DisplayTimeSpan} {DisplayTimeUnit}")]
public record UpdateSchedulerDisplayPreferencesCommand(
    ScheduledTaskType? JobTypeFilter,
    int DisplayTimeSpan,
    SchedulerDisplayTimeUnit DisplayTimeUnit
) : ICommand;
