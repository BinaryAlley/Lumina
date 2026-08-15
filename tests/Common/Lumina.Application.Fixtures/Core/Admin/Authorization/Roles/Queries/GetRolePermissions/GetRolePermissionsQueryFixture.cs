#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.Admin.Authorization.Roles.Queries.GetRolePermissions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.Admin.Authorization.Roles.Queries.GetRolePermissions;

/// <summary>
/// Fixture class for the <see cref="GetRolePermissionsQuery"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetRolePermissionsQueryFixture
{
    /// <summary>
    /// Creates a random valid query to get role permissions.
    /// </summary>
    /// <param name="roleId">Optional. The role Id.</param>
    /// <returns>The created query.</returns>
    public GetRolePermissionsQuery Create(Guid? roleId = null)
    {
        return new Faker<GetRolePermissionsQuery>()
            .CustomInstantiator(f => new GetRolePermissionsQuery(
                roleId ?? Guid.NewGuid()))
            .Generate();
    }

    /// <summary>
    /// Creates a list of <see cref="GetRolePermissionsQuery"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetRolePermissionsQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
