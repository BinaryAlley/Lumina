#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DataAccess.Entities.Authorization;

/// <summary>
/// Represents the association between a user and a role within the authorization system.
/// </summary>
[DebuggerDisplay("User: {User.Username}, Role: {Role.RoleName}")]
public class UserRoleEntity : IStorageEntity, IAuditableEntity
{
    /// <summary>
    /// Gets the Id of the user role.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the Id of the user of the user role.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// Gets the user of the user role.
    /// </summary>
    public required UserEntity User { get; init; }

    /// <summary>
    /// Gets the Id of the role of the user role.
    /// </summary>
    public required Guid RoleId { get; init; }

    /// <summary>
    /// Gets the role of the user role.
    /// </summary>
    public required RoleEntity Role { get; init; }

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
