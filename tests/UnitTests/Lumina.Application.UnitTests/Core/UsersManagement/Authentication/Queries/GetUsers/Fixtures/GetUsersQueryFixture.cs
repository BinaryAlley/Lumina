#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.UsersManagement.Authentication.Queries.GetUsers;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.UsersManagement.Authentication.Queries.GetUsers.Fixtures;

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
    public GetUsersQuery CreateQuery()
    {
        return new GetUsersQuery();
    }
}
