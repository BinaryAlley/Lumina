#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate;
#endregion

namespace Lumina.Application.Common.Mapping.UsersManagement.Users;

/// <summary>
/// Extension methods for converting <see cref="User"/>.
/// </summary>
public static class UserMapping
{
    /// <summary>
    /// Converts <paramref name="domainEntity"/> to <see cref="UserEntity"/>.
    /// </summary>
    /// <param name="domainEntity">The domain entity to be converted.</param>
    /// <param name="repositoryEntity">The repository entity containing sensitive information not mapped to the Domain entity.</param>
    /// <remarks>
    /// Because Authentication and Authorization are Application Layer concerns, the members involved in those concerns are not mirrored in the Domain entity.
    /// However, since Entity Framework updates an entire entity, when mapping from the Domain entity to the DAL entity, one still needs to provide the
    /// values of the non-included members, such that they are not overriden with <see langword="null"/>. Those values should come in the requested <paramref name="repositoryEntity"/>.
    /// </remarks>
    /// <returns>The converted repository entity.</returns>
    public static UserEntity ToRepositoryEntity(this User domainEntity, UserEntity repositoryEntity)
    {
        repositoryEntity.Username = domainEntity.Username;
        return repositoryEntity;
    }
}
