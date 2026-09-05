#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.Scheduling;

/// <summary>
/// Request for getting the history of the executions of the tasks of scheduled jobs.
/// </summary>
/// <param name="From">The inclusive lower bound of the interval for which the history is requested. Optional.</param>
/// <param name="To">The inclusive upper bound of the interval for which the history is requested. Optional.</param>
[DebuggerDisplay("From: {From}, To: {To}")]
public record GetScheduledJobHistoryRequest(
    DateTime? From,
    DateTime? To
);
