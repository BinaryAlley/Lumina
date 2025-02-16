#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Responses.Authorization;
using Mediator;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.UsersManagement.Authorization.Queries.GetUserRole;

/// <summary>
/// Query for retrieving the authorization role of a user identified by <paramref name="UserId"/>.
/// </summary>
/// <param name="UserId">The unique identifier of the user for whom to get the authorization role.</param>
[DebuggerDisplay("UserId: {UserId}")]
public record GetUserRoleQuery(
    Guid? UserId
) : IRequest<ErrorOr<RoleResponse?>>;
