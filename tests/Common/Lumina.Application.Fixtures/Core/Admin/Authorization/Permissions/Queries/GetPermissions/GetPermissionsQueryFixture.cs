#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Admin.Authorization.Permissions.Queries.GetPermissions;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.Admin.Authorization.Permissions.Queries.GetPermissions;

/// <summary>
/// Fixture class for the <see cref="GetPermissionsQuery"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPermissionsQueryFixture
{
    /// <summary>
    /// Creates a random valid query to get permissions.
    /// </summary>
    /// <returns>The created query.</returns>
    public GetPermissionsQuery Create()
    {
        return new GetPermissionsQuery();
    }

    /// <summary>
    /// Creates a list of <see cref="GetPermissionsQuery"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetPermissionsQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
