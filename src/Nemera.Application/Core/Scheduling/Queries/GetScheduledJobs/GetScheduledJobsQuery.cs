#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
using System.Diagnostics;
#endregion

namespace Nemera.Application.Core.Scheduling.Queries.GetScheduledJobs;

/// <summary>
/// Query for getting the list of scheduled jobs.
/// </summary>
[DebuggerDisplay("GetScheduledJobsQuery")]
public record GetScheduledJobsQuery : IQuery;
