#region ========================================================================= USING =====================================================================================
using System;
#endregion

namespace Lumina.Contracts.Models.Common;

/// <summary>
/// Interface for storage models, enforces Id.
/// </summary>
public interface IStorageEntity
{
    #region ==================================================================== PROPERTIES =================================================================================
    /// <summary>
    /// Gets or sets the identifier of the entity.
    /// </summary>
    Guid Id { get; init; }

    /// <summary>
    /// The time and date when the entity was added.
    /// </summary>
    DateTime Created { get; set; }

    /// <summary>
    /// The time and date when the entity was updated.
    /// </summary>
    DateTime? Updated { get; set; }
    #endregion
}