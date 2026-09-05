#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.Scheduling;

/// <summary>
/// Represents a request to fire the task of a scheduled job once, without affecting its execution cycle.
/// </summary>
/// <param name="ScheduledJobId">The unique identifier of the scheduled job whose task is fired. Required.</param>
[DebuggerDisplay("ScheduledJobId: {ScheduledJobId}")]
public record FireScheduledJobRequest(
    Guid ScheduledJobId
);
