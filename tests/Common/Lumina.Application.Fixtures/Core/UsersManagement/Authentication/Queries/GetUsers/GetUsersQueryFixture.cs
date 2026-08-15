#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.UsersManagement.Authentication.Queries.GetUsers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.UsersManagement.Authentication.Queries.GetUsers;

/// <summary>
/// Fixture class for the <see cref="GetUsersQuery"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetUsersQueryFixture
{
    /// <summary>
    /// Creates a random valid query to get users.
    /// </summary>
    /// <returns>The created query.</returns>
    public GetUsersQuery Create()
    {
        return new GetUsersQuery();
    }

    /// <summary>
    /// Creates a list of <see cref="GetUsersQuery"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetUsersQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
