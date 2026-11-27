#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
#endregion

namespace Nemera.Application.Core.Admin.Authorization.Roles.Queries.GetRoles;

/// <summary>
/// Query for retrieving the list of authorization roles.
/// </summary>
public record GetRolesQuery() : IQuery;
