#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.Authorization;

/// <summary>
/// Represents the request model for retrieving the authorization role of a user identified by <paramref name="UserId"/>.
/// </summary>
/// <param name="UserId">The unique identifier of the user for whom to get the authorization role. Required.</param>
[DebuggerDisplay("UserId: {UserId}")]
public record GetUserRoleRequest(
    Guid UserId
);
