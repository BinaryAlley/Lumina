#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.Core.Scheduling.Execution;

/// <summary>
/// Interface for the executor of the task of a scheduled job.
/// </summary>
public interface IScheduledTaskExecutor
{
    /// <summary>
    /// Executes the payload of the task of the scheduled job, asynchronously.
    /// </summary>
    /// <param name="scheduledJob">The scheduled job whose task is executed.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    Task<Result<Success>> ExecutePayloadAsync(ScheduledJob scheduledJob, CancellationToken cancellationToken);
}
