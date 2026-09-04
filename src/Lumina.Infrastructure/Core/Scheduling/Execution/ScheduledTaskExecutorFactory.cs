#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.Scheduling;
using Lumina.Infrastructure.Core.Scheduling.Execution.TaskExecutors;
using Microsoft.Extensions.DependencyInjection;
using System;
#endregion

namespace Lumina.Infrastructure.Core.Scheduling.Execution;

/// <summary>
/// Factory that creates the executor of the task of a scheduled job, based on the type of its task.
/// </summary>
public class ScheduledTaskExecutorFactory : IScheduledTaskExecutorFactory
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduledTaskExecutorFactory"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public ScheduledTaskExecutorFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Creates the executor of the task of a scheduled job, based on the type of its task.
    /// </summary>
    /// <param name="taskType">The type of the task whose executor is created.</param>
    /// <returns>The created task executor.</returns>
    public IScheduledTaskExecutor CreateExecutor(ScheduledTaskType taskType)
    {
        return taskType switch
        {
            ScheduledTaskType.ScanMediaLibraries => _serviceProvider.GetRequiredService<MediaLibraryScanTaskExecutor>(),
            ScheduledTaskType.CleanTemporaryFiles => _serviceProvider.GetRequiredService<TemporaryFilesCleanupTaskExecutor>(),
            ScheduledTaskType.RepairThemes => _serviceProvider.GetRequiredService<RepairThemesTaskExecutor>(),
            ScheduledTaskType.CleanScheduledJobExecutionHistory => _serviceProvider.GetRequiredService<CleanScheduledJobExecutionHistoryTaskExecutor>(),
            _ => throw new ArgumentException($"Unsupported task type: {taskType}", nameof(taskType))
        };
    }
}
