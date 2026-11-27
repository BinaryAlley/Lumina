#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
#endregion

namespace Nemera.Application.Core.UsersManagement.Authentication.Queries.GetUsers;

/// <summary>
/// Query for retrieving the list of users.
/// </summary>
public record GetUsersQuery() : IQuery;
