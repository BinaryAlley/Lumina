#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Responses.Authorization;
using Mediator;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.UsersManagement.Authorization.Queries.GetUserPermissions;

/// <summary>
/// Query for retrieving the authorization permissions of a user identified by <paramref name="UserId"/>.
/// </summary>
/// <param name="UserId">The Id of the user for whom to get the authorization permissions.</param>
[DebuggerDisplay("UserId: {UserId}")]
public record GetUserPermissionsQuery(
    Guid? UserId
) : IRequest<ErrorOr<IEnumerable<PermissionResponse>>>;
