#region ========================================================================= USING =====================================================================================
using System;
#endregion

namespace Lumina.Application.Common.DataAccess.Entities.Common;

/// <summary>
/// Interface for storage models, enforces Id.
/// </summary>
public interface IStorageEntity
{
    /// <summary>
    /// Gets or sets the identifier of the entity.
    /// </summary>
    Guid Id { get; init; }

    /// <summary>
    /// Gets or sets the time and date when the entity was added.
    /// </summary>
    DateTime Created { get; set; }

    /// <summary>
    /// Gets or sets the optional time and date when the entity was updated.
    /// </summary>
    DateTime? Updated { get; set; }
}
