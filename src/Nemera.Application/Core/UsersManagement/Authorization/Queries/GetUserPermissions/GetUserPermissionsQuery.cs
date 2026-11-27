#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.UsersManagement.Authorization.Queries.GetUserPermissions;

/// <summary>
/// Query for retrieving the authorization permissions of a user identified by <paramref name="UserId"/>.
/// </summary>
/// <param name="UserId">The unique identifier of the user for whom to get the authorization permissions.</param>
[DebuggerDisplay("UserId: {UserId}")]
public record GetUserPermissionsQuery(
    Guid? UserId
) : IQuery;
