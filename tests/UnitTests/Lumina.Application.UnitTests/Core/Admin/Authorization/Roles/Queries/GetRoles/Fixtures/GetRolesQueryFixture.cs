#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Admin.Authorization.Roles.Queries.GetRoles;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.Admin.Authorization.Roles.Queries.GetRoles.Fixtures;

/// <summary>
/// Fixture class for the <see cref="GetRolesQuery"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetRolesQueryFixture
{
    /// <summary>
    /// Creates a random valid query to get roles.
    /// </summary>
    /// <returns>The created query.</returns>
    public GetRolesQuery CreateQuery()
    {
        return new GetRolesQuery();
    }
}
