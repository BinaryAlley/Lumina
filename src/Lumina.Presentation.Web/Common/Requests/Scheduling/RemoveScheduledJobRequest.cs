#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.Scheduling;

/// <summary>
/// Request for removing a scheduled job.
/// </summary>
/// <param name="ScheduledJobId">The unique identifier of the scheduled job to remove. Required.</param>
[DebuggerDisplay("ScheduledJobId: {ScheduledJobId}")]
public record RemoveScheduledJobRequest(
    Guid ScheduledJobId
);
