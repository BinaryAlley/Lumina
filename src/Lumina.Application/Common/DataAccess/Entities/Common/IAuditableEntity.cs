#region ========================================================================= USING =====================================================================================
using System;
#endregion

namespace Lumina.Application.Common.DataAccess.Entities.Common;

/// <summary>
/// Interface for the properties required for auditing changes to an entity.
/// </summary>
public interface IAuditableEntity
{
    /// <summary>
    /// Gets or sets the time and date when the entity was added.
    /// </summary>
    DateTime CreatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the Id of the user that created the entity.
    /// </summary>
    Guid CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the optional time and date when the entity was updated.
    /// </summary>
    DateTime? UpdatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the optional Id of the user that updated the entity.
    /// </summary>
    Guid? UpdatedBy { get; set; }
}
