#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Enums.Authorization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.Authorization;

/// <summary>
/// Represents the response model for an account authorization request.
/// </summary>
/// <param name="UserId">The unique identifier of the user for whom the authorization is retrieved.</param>
/// <param name="Role">The collection of roles associated to the user.</param>
/// <param name="Permissions">The collection of permissions associated to the user.</param>
[DebuggerDisplay("UserId: {UserId}")]
public record AuthorizationResponse(
    Guid UserId,
    string? Role,
    IReadOnlySet<AuthorizationPermission> Permissions
);
