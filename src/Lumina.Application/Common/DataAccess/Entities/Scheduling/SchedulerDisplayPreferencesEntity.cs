#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Domain.SharedKernel.Common.Enums.Scheduling;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DataAccess.Entities.Scheduling;

/// <summary>
/// Repository entity for the display preferences of the scheduler page of a user.
/// </summary>
[DebuggerDisplay("UserId: {UserId}, DisplayTimeSpan: {DisplayTimeSpan}")]
public class SchedulerDisplayPreferencesEntity : IStorageEntity, IAuditableEntity
{
    /// <summary>
    /// Gets or sets the Id of the display preferences.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets or sets the Id of the user that owns the display preferences.
    /// </summary>
    public required Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the type of the scheduled job tasks whose executions are shown on the scheduler page, or <see langword="null"/> when all of them are shown.
    /// </summary>
    public ScheduledTaskType? JobTypeFilter { get; set; }

    /// <summary>
    /// Gets or sets the time span, expressed in <see cref="DisplayTimeUnit"/>, that the scheduler page shows.
    /// </summary>
    public required int DisplayTimeSpan { get; set; }

    /// <summary>
    /// Gets or sets the unit in which the displayed time span of the scheduler page is expressed.
    /// </summary>
    public required SchedulerDisplayTimeUnit DisplayTimeUnit { get; set; }

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
