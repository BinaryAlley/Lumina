#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.Admin.Authorization.Roles.Queries.GetRolePermissions;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.Admin.Authorization.Roles.Queries.GetRolePermissions.Fixtures;

/// <summary>
/// Fixture class for the <see cref="GetRolePermissionsQuery"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetRolePermissionsQueryFixture
{
    /// <summary>
    /// Creates a random valid query to get role permissions.
    /// </summary>
    /// <returns>The created query.</returns>
    public GetRolePermissionsQuery CreateQuery()
    {
        return new Faker<GetRolePermissionsQuery>()
            .CustomInstantiator(f => new GetRolePermissionsQuery(
                Guid.NewGuid()))
            .Generate();
    }
}
