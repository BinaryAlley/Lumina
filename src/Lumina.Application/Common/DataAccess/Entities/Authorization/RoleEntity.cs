#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Domain.Common.Enums.Authorization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DataAccess.Entities.Authorization;

/// <summary>
/// Repository entity for a permission in the authorization system.
/// </summary>
[DebuggerDisplay("RoleName: {RoleName}")]
public class RoleEntity : IStorageEntity, IAuditableEntity
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
    /// Gets or sets the collection of user roles associations that include this role.
    /// </summary>
    public ICollection<UserRoleEntity> UserRoles { get; set; } = [];

    /// <summary>
    /// Gets or sets the collection of role permission associations that include this role.
    /// </summary>
    public ICollection<RolePermissionEntity> RolePermissions { get; set; } = [];

    /// <summary>
    /// Gets or sets the time and date when the entity was added.
    /// </summary>
    public DateTime CreatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the Id of the user that created the entity.
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the optional time and date when the entity was updated.
    /// </summary>
    public DateTime? UpdatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the optional Id of the user that updated the entity.
    /// </summary>
    public Guid? UpdatedBy { get; set; }
}
