#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DataAccess.Entities.Authorization;

/// <summary>
/// Represents a permission that can be assigned to roles or users within the authorization system.
/// </summary>
[DebuggerDisplay("PermissionName: {PermissionName}")]
public class PermissionEntity : IStorageEntity, IAuditableEntity
{
    /// <summary>
    /// Gets the Id of the permission.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the name of the permission.
    /// </summary>
    public required string PermissionName { get; init; }

    /// <summary>
    /// Gets the collection of role permission associations that include this permission.
    /// </summary>
    public required ICollection<RolePermissionEntity> RolePermissions { get; init; } = [];

    /// <summary>
    /// Gets the collection of user permission associations that include this permission.
    /// </summary>
    public required ICollection<UserPermissionEntity> UserPermissions { get; init; } = [];

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
    public Guid? UpdatedBy { get; set; }
}
