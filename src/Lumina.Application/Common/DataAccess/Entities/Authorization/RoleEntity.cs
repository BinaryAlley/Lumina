#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DataAccess.Entities.Authorization;

/// <summary>
/// Repository entity for a permission in the authorization system.
/// </summary>
[DebuggerDisplay("RoleName: {RoleName}")]
public class RoleEntity : IStorageEntity
{
    /// <summary>
    /// Gets the Id of the role.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the name of the role.
    /// </summary>
    public required string RoleName { get; init; }

    /// <summary>
    /// Gets the collection of user roles associations that include this role.
    /// </summary>
    public required ICollection<UserRoleEntity> UserRoles { get; init; } = [];

    /// <summary>
    /// Gets the collection of role permission associations that include this role.
    /// </summary>
    public required ICollection<RolePermissionEntity> RolePermissions { get; init; } = [];

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
