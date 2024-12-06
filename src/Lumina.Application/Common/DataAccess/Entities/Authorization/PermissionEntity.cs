#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Domain.Common.Enums.Authorization;
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
    public required AuthorizationPermission PermissionName { get; init; }

    /// <summary>
    /// Gets the description of the permission.
    /// </summary>
    public string? PermissionDescription { get; set; }

    /// <summary>
    /// Gets or sets the collection of role permission associations that include this permission.
    /// </summary>
    public ICollection<RolePermissionEntity> RolePermissions { get; set; } = [];

    /// <summary>
    /// Gets or sets the collection of user permission associations that include this permission.
    /// </summary>
    public ICollection<UserPermissionEntity> UserPermissions { get; set; } = [];

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
