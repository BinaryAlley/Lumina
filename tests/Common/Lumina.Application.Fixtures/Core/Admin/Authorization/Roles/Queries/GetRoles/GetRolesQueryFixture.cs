#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Admin.Authorization.Roles.Queries.GetRoles;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.Admin.Authorization.Roles.Queries.GetRoles;

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
    public GetRolesQuery Create()
    {
        return new GetRolesQuery();
    }

    /// <summary>
    /// Creates a list of <see cref="GetRolesQuery"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetRolesQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
