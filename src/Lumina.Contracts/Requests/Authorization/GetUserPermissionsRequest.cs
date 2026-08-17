#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.Authorization;

/// <summary>
/// Represents the request model for retrieving the authorization permissions of a user identified by <paramref name="UserId"/>.
/// </summary>
/// <param name="UserId">The unique identifier of the user for whom to get the authorization permissions. Required.</param>
[DebuggerDisplay("UserId: {UserId}")]
public record GetUserPermissionsRequest(
    Guid? UserId
);
