#region ========================================================================= USING =====================================================================================
using System;
#endregion

namespace Lumina.Contracts.Requests.Authorization;

/// <summary>
/// Represents the request model for retrieving account authorization information.
/// </summary>
/// <param name="UserId">The Id of the user for whom to get the authorization. Required.</param>
public record GetAuthorizationRequest(
    Guid? UserId
);
