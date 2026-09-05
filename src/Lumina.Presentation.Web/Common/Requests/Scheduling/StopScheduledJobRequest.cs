#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.Scheduling;

/// <summary>
/// Request for stopping the execution cycle of a scheduled job.
/// </summary>
/// <param name="ScheduledJobId">The unique identifier of the scheduled job whose execution cycle is stopped. Required.</param>
[DebuggerDisplay("ScheduledJobId: {ScheduledJobId}")]
public record StopScheduledJobRequest(
    Guid ScheduledJobId
);
