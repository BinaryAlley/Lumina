#region ========================================================================= USING =====================================================================================
using System;
#endregion

namespace Lumina.Contracts.Requests.Authorization;

/// <summary>
/// Represents the request model for retrieving the authorization permissions of a user identified by <paramref name="UserId"/>.
/// </summary>
/// <param name="UserId">The Id of the user for whom to get the authorization permissions. Required.</param>
public record GetUserPermissionsRequest(
    Guid? UserId
);
