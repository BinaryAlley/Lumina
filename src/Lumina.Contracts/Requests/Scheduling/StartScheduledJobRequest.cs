#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.Scheduling;

/// <summary>
/// Represents a request to start the execution cycle of a scheduled job.
/// </summary>
/// <param name="ScheduledJobId">The unique identifier of the scheduled job whose execution cycle is started. Required.</param>
[DebuggerDisplay("ScheduledJobId: {ScheduledJobId}")]
public record StartScheduledJobRequest(
    Guid ScheduledJobId
);
