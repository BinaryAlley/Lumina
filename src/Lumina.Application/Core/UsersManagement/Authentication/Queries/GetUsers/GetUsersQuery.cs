#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Responses.UsersManagement.Users;
using Mediator;
using System.Collections.Generic;
#endregion

namespace Lumina.Application.Core.UsersManagement.Authentication.Queries.GetUsers;

/// <summary>
/// Query for retrieving the list of users.
/// </summary>
public record GetUsersQuery() : IRequest<ErrorOr<IEnumerable<UserResponse>>>;
