#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.Core.Scheduling.Execution.TaskExecutors;

/// <summary>
/// Task executor that cleans the technical data that is no longer valid.
/// </summary>
public class TechnicalDataCleanupTaskExecutor : IScheduledTaskExecutor
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TechnicalDataCleanupTaskExecutor> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TechnicalDataCleanupTaskExecutor"/> class.
    /// </summary>
    /// <param name="logger">Injected service for logging.</param>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    public TechnicalDataCleanupTaskExecutor(ILogger<TechnicalDataCleanupTaskExecutor> logger, IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Executes the cleanup of the technical data that is no longer valid.
    /// </summary>
    /// <param name="scheduledJob">The scheduled job whose task is executed.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Success>> ExecutePayloadAsync(ScheduledJob scheduledJob, CancellationToken cancellationToken)
    {
        Result<Updated> failInterruptedScansResult = await _unitOfWork.LibraryScanRepository.FailInterruptedScansAsync(cancellationToken).ConfigureAwait(false);
        if (failInterruptedScansResult.IsFailure)
            return failInterruptedScansResult.Errors;

        _logger.LogInformation("Cleaned the technical data on behalf of the scheduled job '{ScheduledJobName}'.", scheduledJob.Name);
        return Result.Success;
    }
}
