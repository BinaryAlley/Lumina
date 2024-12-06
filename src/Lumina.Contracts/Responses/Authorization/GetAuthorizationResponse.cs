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
/// <param name="UserId">The Id of the user for whom the authorization is retrieved.</param>
/// <param name="Roles">The collection of roles associated to the user.</param>
/// <param name="Permissions">The collection of permissions associated to the user.</param>
[DebuggerDisplay("UserId: {UserId}")]
public record GetAuthorizationResponse(
    Guid UserId,
    IReadOnlySet<AuthorizationRole> Roles,
    IReadOnlySet<AuthorizationPermission> Permissions
);
