#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Domain.SharedKernel.Common.Enums.Scheduling;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DataAccess.Entities.Scheduling;

/// <summary>
/// Repository entity for an execution of the task of a scheduled job.
/// </summary>
[DebuggerDisplay("Id: {Id}, ScheduledJobId: {ScheduledJobId}")]
public class ScheduledJobExecutionEntity : IStorageEntity, IAuditableEntity
{
    /// <summary>
    /// Gets the Id of the execution of the task of a scheduled job.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the Id of the scheduled job whose task was executed.
    /// </summary>
    public required Guid ScheduledJobId { get; init; }

    /// <summary>
    /// Gets the type of the task that was executed.
    /// </summary>
    public required ScheduledTaskType TaskType { get; init; }

    /// <summary>
    /// Gets a value indicating whether the execution was triggered by the execution cycle of the scheduled job, or it was a one time execution.
    /// </summary>
    public required bool IsCycleRun { get; init; }

    /// <summary>
    /// Gets the date and time when the execution started.
    /// </summary>
    public required DateTime StartedOnUtc { get; init; }

    /// <summary>
    /// Gets the optional date and time when the execution completed.
    /// </summary>
    public DateTime? CompletedOnUtc { get; init; }

    /// <summary>
    /// Gets a value indicating whether the execution cycle of the scheduled job was active when the execution started.
    /// </summary>
    /// <remarks>
    /// A job whose execution cycle is active stays in its active state while a manually fired one time execution runs, so an
    /// interrupted execution of such a job must return it to its active state at the next startup; the flag keeps that fact
    /// separate from <see cref="IsCycleRun"/>, which records how the execution was triggered for the audit trail.
    /// </remarks>
    public required bool WasCycleActive { get; init; }

    /// <summary>
    /// Gets or sets the time and date when the entity was added.
    /// </summary>
    public required DateTime CreatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the Id of the user that created the entity.
    /// </summary>
    public required Guid CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the optional time and date when the entity was updated.
    /// </summary>
    public DateTime? UpdatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the optional Id of the user that updated the entity.
    /// </summary>
    public required Guid? UpdatedBy { get; set; }
}
