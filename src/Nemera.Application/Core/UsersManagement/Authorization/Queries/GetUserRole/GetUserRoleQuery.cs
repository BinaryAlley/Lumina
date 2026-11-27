#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
using System;
using System.Diagnostics;
#endregion

namespace Nemera.Application.Core.UsersManagement.Authorization.Queries.GetUserRole;

/// <summary>
/// Query for retrieving the authorization role of a user identified by <paramref name="UserId"/>.
/// </summary>
/// <param name="UserId">The unique identifier of the user for whom to get the authorization role.</param>
[DebuggerDisplay("UserId: {UserId}")]
public record GetUserRoleQuery(
    Guid? UserId
) : IQuery;
