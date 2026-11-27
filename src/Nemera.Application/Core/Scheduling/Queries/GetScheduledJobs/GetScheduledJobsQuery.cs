#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.Scheduling.Queries.GetScheduledJobs;

/// <summary>
/// Query for getting the list of scheduled jobs.
/// </summary>
[DebuggerDisplay("GetScheduledJobsQuery")]
public record GetScheduledJobsQuery : IQuery;
