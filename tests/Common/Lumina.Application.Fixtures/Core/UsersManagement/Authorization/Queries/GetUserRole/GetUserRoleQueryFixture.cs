#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.UsersManagement.Authorization.Queries.GetUserRole;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.UsersManagement.Authorization.Queries.GetUserRole;

/// <summary>
/// Fixture class for the <see cref="GetUserRoleQuery"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetUserRoleQueryFixture
{
    /// <summary>
    /// Creates a random valid query to get a user's role.
    /// </summary>
    /// <param name="userId">Optional. The user Id.</param>
    /// <returns>The created query.</returns>
    public GetUserRoleQuery Create(Guid? userId = null)
    {
        return new Faker<GetUserRoleQuery>()
            .CustomInstantiator(f => new GetUserRoleQuery(
                userId ?? Guid.NewGuid()))
            .Generate();
    }

    /// <summary>
    /// Creates a list of <see cref="GetUserRoleQuery"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="GetUserRoleQuery"/> instances.</returns>
    public List<GetUserRoleQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
