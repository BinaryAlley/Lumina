#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.UsersManagement.Authorization.Queries.GetUserPermissions;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.UsersManagement.Authorization.Queries.GetUserPermissions.Fixtures;

/// <summary>
/// Fixture class for the <see cref="GetUserPermissionsQuery"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetUserPermissionsQueryFixture
{
    /// <summary>
    /// Creates a random valid query to get user permissions.
    /// </summary>
    /// <returns>The created query.</returns>
    public GetUserPermissionsQuery CreateQuery()
    {
        return new Faker<GetUserPermissionsQuery>()
            .CustomInstantiator(f => new GetUserPermissionsQuery(
                Guid.NewGuid()))
            .Generate();
    }
}
