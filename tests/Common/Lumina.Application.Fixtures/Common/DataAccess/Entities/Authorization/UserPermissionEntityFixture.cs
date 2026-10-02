#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using System;
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
    /// <param name="createdOnUtc">Optional. The time and date when the association was created.</param>
    /// <param name="createdBy">Optional. The Id of the user that created the association.</param>
    /// <returns>The created <see cref="UserPermissionEntity"/>.</returns>
    public UserPermissionEntity Create(
        UserEntity user,
        PermissionEntity permission,
        DateTime? createdOnUtc = null,
        Guid? createdBy = null)
    {
        return new Faker<UserPermissionEntity>()
            .CustomInstantiator(f => new UserPermissionEntity
            {
                Id = f.Random.Guid(),
                UserId = user.Id,
                User = user,
                PermissionId = permission.Id,
                Permission = permission,
                CreatedOnUtc = createdOnUtc ?? f.Date.Past(),
                CreatedBy = createdBy ?? user.Id,
                UpdatedOnUtc = f.Random.Bool() ? f.Date.Recent() : null,
                UpdatedBy = f.Random.Bool() ? user.Id : null
            })
            .Generate();
    }

}
