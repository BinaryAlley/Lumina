#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.Scheduling;

/// <summary>
/// Represents a request to remove a scheduled job.
/// </summary>
/// <param name="ScheduledJobId">The unique identifier of the scheduled job to remove. Required.</param>
[DebuggerDisplay("ScheduledJobId: {ScheduledJobId}")]
public record RemoveScheduledJobRequest(
    Guid ScheduledJobId
);
