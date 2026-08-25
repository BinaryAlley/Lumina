#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Domain.SharedKernel.Common.Enums.Authorization;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DataAccess.Entities.Authorization;

/// <summary>
/// Fixture class for the <see cref="UserAuthorizationEntity"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UserAuthorizationEntityFixture
{
    /// <summary>
    /// Creates a random valid authorization entity.
    /// </summary>
    /// <param name="userId">Optional. The user Id to use. If null, a random GUID will be generated.</param>
    /// <param name="isAdmin">Whether the user should have admin role.</param>
    /// <param name="role">Optional. The role associated to the user. If null, it is derived from the <paramref name="isAdmin"/> flag.</param>
    /// <param name="permissions">Optional. The permissions associated to the user. If null, a default set is generated.</param>
    /// <returns>The created entity.</returns>
    public UserAuthorizationEntity Create(
        Guid? userId = null,
        bool isAdmin = false,
        string? role = null,
        IReadOnlySet<AuthorizationPermission>? permissions = null)
    {
        string resolvedRole = role ?? (isAdmin ? "Admin" : string.Empty);

        return new UserAuthorizationEntity
        {
            UserId = userId ?? Guid.NewGuid(),
            Role = resolvedRole,
            Permissions = permissions ?? new HashSet<AuthorizationPermission>
            {
                AuthorizationPermission.CanViewUsers,
                AuthorizationPermission.CanRegisterUsers
            }
        };
    }

    /// <summary>
    /// Creates a list of <see cref="UserAuthorizationEntity"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <param name="isAdmin">Whether the users should have admin role.</param>
    /// <returns>List of configured <see cref="UserAuthorizationEntity"/> instances.</returns>
    public List<UserAuthorizationEntity> CreateMany(int count = 3, bool isAdmin = false)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create(isAdmin: isAdmin))];
    }
}
