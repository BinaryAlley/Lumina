#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.UsersManagement.Authorization.Queries.GetUserRole;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.UsersManagement.Authorization.Queries.GetUserRole.Fixtures;

/// <summary>
/// Fixture class for the <see cref="GetUserRoleQuery"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetUserRoleQueryFixture
{
    /// <summary>
    /// Creates a random valid query to get a user's role.
    /// </summary>
    /// <returns>The created query.</returns>
    public GetUserRoleQuery CreateQuery()
    {
        return new Faker<GetUserRoleQuery>()
            .CustomInstantiator(f => new GetUserRoleQuery(
                Guid.NewGuid()))
            .Generate();
    }
}
