#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Core.UsersManagement.Authorization.Queries.GetAuthorization;
using Lumina.Domain.Common.Enums.Authorization;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.UsersManagement.Authorization.Queries.GetAuthorization.Fixtures;

/// <summary>
/// Fixture class for the <see cref="GetAuthorizationQuery"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetAuthorizationQueryFixture
{
    /// <summary>
    /// Creates a random valid query to get authorization details.
    /// </summary>
    /// <param name="userId">Optional user Id to use. If null, a random GUID will be generated.</param>
    /// <returns>The created query.</returns>
    public static GetAuthorizationQuery CreateGetAuthorizationQuery(Guid? userId = null)
    {
        return new Faker<GetAuthorizationQuery>()
            .CustomInstantiator(f => new GetAuthorizationQuery(
                userId ?? f.Random.Guid()
            ))
            .Generate();
    }

    /// <summary>
    /// Creates a random valid authorization entity.
    /// </summary>
    /// <param name="userId">Optional user Id to use. If null, a random GUID will be generated.</param>
    /// <param name="isAdmin">Whether the user should have admin role.</param>
    /// <returns>The created entity.</returns>
    public static UserAuthorizationEntity CreateUserAuthorizationEntity(Guid? userId = null, bool isAdmin = false)
    {
        IReadOnlySet<string> roles = isAdmin
            ? ["Admin"]
            : new HashSet<string>();

        return new UserAuthorizationEntity
        {
            UserId = userId ?? Guid.NewGuid(),
            Roles = roles,
            Permissions = new HashSet<AuthorizationPermission>
            {
                AuthorizationPermission.CanViewUsers,
                AuthorizationPermission.CanRegisterUsers
            }
        };
    }
}
