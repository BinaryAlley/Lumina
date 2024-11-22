#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DataAccess.Entities.Authorization;

/// <summary>
/// Represents the association between a user and a permission within the authorization system.
/// </summary>
[DebuggerDisplay("User: {User.Username}, Permission: {Permission.PermissionName}")]
public class UserPermissionEntity : IStorageEntity, IAuditableEntity
{
    /// <summary>
    /// Gets the Id of the user permission.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the Id of the user of the user permission.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// Gets the user of the user permission.
    /// </summary>
    public required UserEntity User { get; init; }

    /// <summary>
    /// Gets the Id of the permission of the user permission.
    /// </summary>
    public required Guid PermissionId { get; init; }

    /// <summary>
    /// Gets the permission of the user permission.
    /// </summary>
    public required PermissionEntity Permission { get; init; }

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
