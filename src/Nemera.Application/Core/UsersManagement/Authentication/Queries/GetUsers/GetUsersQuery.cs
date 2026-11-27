#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
#endregion

namespace Lumina.Application.Core.UsersManagement.Authentication.Queries.GetUsers;

/// <summary>
/// Query for retrieving the list of users.
/// </summary>
public record GetUsersQuery() : IQuery;
