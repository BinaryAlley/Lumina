#region ========================================================================= USING =====================================================================================
using System;
#endregion

namespace Lumina.Contracts.Responses.Authorization;

/// <summary>
/// Represents the response model for a request for an authorization role.
/// </summary>
/// <param name="Id">The Id of the requested authorization role.</param>
/// <param name="RoleName">The requested authorization role.</param>
public record RoleResponse(
    Guid Id,
    string RoleName
);
