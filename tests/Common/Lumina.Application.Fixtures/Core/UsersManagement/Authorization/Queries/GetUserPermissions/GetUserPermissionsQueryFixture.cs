#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.UsersManagement.Authorization.Queries.GetUserPermissions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.UsersManagement.Authorization.Queries.GetUserPermissions;

/// <summary>
/// Fixture class for the <see cref="GetUserPermissionsQuery"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetUserPermissionsQueryFixture
{
    /// <summary>
    /// Creates a random valid query to get user permissions.
    /// </summary>
    /// <param name="userId">Optional. The user Id.</param>
    /// <returns>The created query.</returns>
    public GetUserPermissionsQuery Create(Guid? userId = null)
    {
        return new Faker<GetUserPermissionsQuery>()
            .CustomInstantiator(f => new GetUserPermissionsQuery(
                userId ?? Guid.NewGuid()))
            .Generate();
    }

    /// <summary>
    /// Creates a list of <see cref="GetUserPermissionsQuery"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="GetUserPermissionsQuery"/> instances.</returns>
    public List<GetUserPermissionsQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
