#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Enums.Authorization;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Models.Authorization;

/// <summary>
/// Represents the response model for an account authorization request.
/// </summary>
/// <param name="UserId">The Id of the user for whom the authorization is retrieved.</param>
/// <param name="Role">The role associated to the user.</param>
/// <param name="Permissions">The collection of permissions associated to the user.</param>
[DebuggerDisplay("UserId: {UserId}")]
public record GetAuthorizationResponse(
    Guid UserId,
    string? Role,
    AuthorizationPermission[] Permissions
);
