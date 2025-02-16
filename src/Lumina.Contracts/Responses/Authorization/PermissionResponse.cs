#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Enums.Authorization;
using System;
#endregion

namespace Lumina.Contracts.Responses.Authorization;

/// <summary>
/// Represents the response model for a request for an of authorization permission.
/// </summary>
/// <param name="Id">The unique identifier of the requested authorization permission.</param>
/// <param name="PermissionName">The requested authorization permission.</param>
public record PermissionResponse(
    Guid Id,
    AuthorizationPermission PermissionName
);
