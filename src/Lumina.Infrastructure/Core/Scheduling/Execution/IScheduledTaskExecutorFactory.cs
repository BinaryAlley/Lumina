#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.Scheduling;
#endregion

namespace Lumina.Infrastructure.Core.Scheduling.Execution;

/// <summary>
/// Interface for the factory that creates the executor of the task of a scheduled job, based on the type of its task.
/// </summary>
public interface IScheduledTaskExecutorFactory
{
    /// <summary>
    /// Creates the executor of the task of a scheduled job, based on the type of its task.
    /// </summary>
    /// <param name="taskType">The type of the task whose executor is created.</param>
    /// <returns>The created task executor.</returns>
    IScheduledTaskExecutor CreateExecutor(ScheduledTaskType taskType);
}
