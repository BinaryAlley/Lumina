#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.Scheduling.Queries.GetScheduledJobHistory;

/// <summary>
/// Query for getting the history of the executions of the tasks of scheduled jobs.
/// </summary>
/// <param name="From">The optional inclusive lower bound of the interval for which the history is requested.</param>
/// <param name="To">The optional inclusive upper bound of the interval for which the history is requested.</param>
[DebuggerDisplay("From: {From}, To: {To}")]
public record GetScheduledJobHistoryQuery(
    DateTime? From,
    DateTime? To
) : IQuery;
