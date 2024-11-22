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
}
