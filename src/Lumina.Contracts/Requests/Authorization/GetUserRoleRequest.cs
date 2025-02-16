#region ========================================================================= USING =====================================================================================
using System;
#endregion

namespace Lumina.Contracts.Requests.Authorization;

/// <summary>
/// Represents the request model for retrieving the authorization role of a user identified by <paramref name="UserId"/>.
/// </summary>
/// <param name="UserId">The unique identifier of the user for whom to get the authorization role. Required.</param>
public record GetUserRoleRequest(
    Guid? UserId
);
