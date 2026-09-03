#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.Scheduling;

/// <summary>
/// Request for firing the task of a scheduled job once.
/// </summary>
/// <param name="ScheduledJobId">The unique identifier of the scheduled job whose task is fired. Required.</param>
[DebuggerDisplay("ScheduledJobId: {ScheduledJobId}")]
public record FireScheduledJobRequest(
    Guid ScheduledJobId
);
