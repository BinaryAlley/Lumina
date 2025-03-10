#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Enums.Authorization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DataAccess.Entities.Authorization;

/// <summary>
/// Data Transfer Object containing all the authorization info of a user.
/// </summary>
[DebuggerDisplay("UserId: {UserId}")]
public class UserAuthorizationEntity
{
    /// <summary>
    /// Gets the Id of the user.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// Gets the role associated to the user.
    /// </summary>
    public required string? Role { get; init; }

    /// <summary>
    /// Gets the collection of permissions associated to the user.
    /// </summary>
    public required IReadOnlySet<AuthorizationPermission> Permissions { get; init; }
}
