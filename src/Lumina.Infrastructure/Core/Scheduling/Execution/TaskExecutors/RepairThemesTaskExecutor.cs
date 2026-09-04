#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Themes;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate;
using Lumina.Infrastructure.Core.Themes;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.Core.Scheduling.Execution.TaskExecutors;

/// <summary>
/// Task executor that repairs the installed themes whose files are missing.
/// </summary>
public class RepairThemesTaskExecutor : IScheduledTaskExecutor
{
    private readonly IThemeService _themeService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RepairThemesTaskExecutor> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RepairThemesTaskExecutor"/> class.
    /// </summary>
    /// <param name="themeService">Injected service for the server-side storage and serving of theme packs.</param>
    /// <param name="logger">Injected service for logging.</param>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    public RepairThemesTaskExecutor(IThemeService themeService, ILogger<RepairThemesTaskExecutor> logger, IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _themeService = themeService;
        _logger = logger;
    }

    /// <summary>
    /// Repairs the installed themes whose files are missing, reinstalling the files of the bundled themes from their shipped archives.
    /// </summary>
    /// <param name="scheduledJob">The scheduled job whose task is executed.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Success>> ExecutePayloadAsync(ScheduledJob scheduledJob, CancellationToken cancellationToken)
    {
        // Repairs are best effort, so a failure to repair one theme is logged inside the synchronizer and the remaining themes are still repaired.
        try
        {
            await ThemeSynchronizer.SynchronizeAsync(_themeService, _unitOfWork, _logger, cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("Repaired the themes on behalf of the scheduled job '{ScheduledJobName}'.", scheduledJob.Name);
            return Result.Success;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "The themes could not be repaired on behalf of the scheduled job '{ScheduledJobName}'.", scheduledJob.Name);
            return Error.Failure(description: nameof(RepairThemesTaskExecutor));
        }
    }
}
