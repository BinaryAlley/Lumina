#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.UsersManagement.Authorization.Queries.GetAuthorization;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.UsersManagement.Authorization.Queries.GetAuthorization;

/// <summary>
/// Fixture class for the <see cref="GetAuthorizationQuery"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetAuthorizationQueryFixture
{
    /// <summary>
    /// Creates a random valid query to get authorization details.
    /// </summary>
    /// <param name="userId">Optional. The user Id to use. If null, a random GUID will be generated.</param>
    /// <returns>The created query.</returns>
    public GetAuthorizationQuery Create(Guid? userId = null)
    {
        return new Faker<GetAuthorizationQuery>()
            .CustomInstantiator(f => new GetAuthorizationQuery(
                userId ?? f.Random.Guid()
            ))
            .Generate();
    }

    /// <summary>
    /// Creates a list of <see cref="GetAuthorizationQuery"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="GetAuthorizationQuery"/> instances.</returns>
    public List<GetAuthorizationQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
