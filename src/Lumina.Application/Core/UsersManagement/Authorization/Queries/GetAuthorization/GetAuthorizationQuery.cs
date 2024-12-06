#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Responses.Authorization;
using Mediator;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.UsersManagement.Authorization.Queries.GetAuthorization;

/// <summary>
/// Query for retrieving the authorization roles and permissions of an account.
/// </summary>
/// <param name="UserId">The Id of the user for whom to get the authorization.</param>
[DebuggerDisplay("UserId: {UserId}")]
public record GetAuthorizationQuery(
    Guid? UserId
) : IRequest<ErrorOr<GetAuthorizationResponse>>;
