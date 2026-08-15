#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.Fixtures.Common.DataAccess.Entities.Authorization;

/// <summary>
/// Fixture class for the <see cref="UserPermissionEntity"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UserPermissionEntityFixture
{
    /// <summary>
    /// Creates a random valid <see cref="UserPermissionEntity"/>.
    /// </summary>
    /// <param name="user">The user the permission is granted to.</param>
    /// <param name="permission">The permission granted to the user.</param>
    /// <returns>The created <see cref="UserPermissionEntity"/>.</returns>
    public UserPermissionEntity Create(UserEntity user, PermissionEntity permission)
    {
        return new Faker<UserPermissionEntity>()
            .CustomInstantiator(f => new UserPermissionEntity
            {
                Id = f.Random.Guid(),
                UserId = user.Id,
                User = user,
                PermissionId = permission.Id,
                Permission = permission,
                CreatedOnUtc = f.Date.Past(),
                CreatedBy = user.Id,
                UpdatedOnUtc = f.Random.Bool() ? f.Date.Recent() : null,
                UpdatedBy = f.Random.Bool() ? user.Id : null
            })
            .Generate();
    }

}
