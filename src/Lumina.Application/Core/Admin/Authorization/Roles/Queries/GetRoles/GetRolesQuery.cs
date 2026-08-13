#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
#endregion

namespace Lumina.Application.Core.Admin.Authorization.Roles.Queries.GetRoles;

/// <summary>
/// Query for retrieving the list of authorization roles.
/// </summary>
public record GetRolesQuery() : IQuery;
