#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Entities.UsersManagement;
using Lumina.Contracts.Responses.UsersManagement.Users;
#endregion

namespace Lumina.Application.Common.Mapping.UsersManagement.Users;

/// <summary>
/// Extension methods for converting <see cref="UserEntity"/>.
/// </summary>
public static class UserEntityMapping
{
    /// <summary>
    /// Converts <paramref name="repositoryEntity"/> to <see cref="UserResponse"/>.
    /// </summary>
    /// <param name="repositoryEntity">The repository entity to be converted.</param>
    /// <returns>The converted response entity.</returns>
    public static UserResponse ToResponse(this UserEntity repositoryEntity)
    {
        return new UserResponse(
            repositoryEntity.Id,
            repositoryEntity.Username,
            repositoryEntity.Created,
            repositoryEntity.Updated
        );
    }
}
