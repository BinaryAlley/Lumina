#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Contracts.DTO.Authentication;
using Lumina.Contracts.Responses.Authorization;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.Authorization;

/// <summary>
/// Extension methods for converting <see cref="RoleEntity"/>.
/// </summary>
public static class RoleEntityMapping
{
    /// <summary>
    /// Converts <paramref name="repositoryEntity"/> to <see cref="RoleResponse"/>.
    /// </summary>
    /// <param name="repositoryEntity">The repository entity to be converted.</param>
    /// <returns>The converted response.</returns>
    public static RoleResponse ToResponse(this RoleEntity repositoryEntity)
    {
        return new RoleResponse(
            repositoryEntity.Id,
            repositoryEntity.RoleName
        );
    }

    /// <summary>
    /// Converts <paramref name="repositoryEntity"/> to <see cref="RoleResponse"/>.
    /// </summary>
    /// <param name="repositoryEntity">The repository entity to be converted.</param>
    /// <returns>The converted response.</returns>
    public static RolePermissionsResponse ToRolePermissionsResponse(this RoleEntity repositoryEntity)
    {
        return new RolePermissionsResponse(
            new RoleDto(repositoryEntity.Id, repositoryEntity.RoleName),
            repositoryEntity.RolePermissions.Select(rolePermission => rolePermission.ToResponse()).ToArray()
        );
    }

    /// <summary>
    /// Converts <paramref name="repositoryEntities"/> to a collection of <see cref="RoleResponse"/>.
    /// </summary>
    /// <param name="repositoryEntities">The repository entities to be converted.</param>
    /// <returns>The converted responses.</returns>
    public static IEnumerable<RoleResponse> ToResponses(this IEnumerable<RoleEntity> repositoryEntities)
    {
        return repositoryEntities.Select(role => role.ToResponse());
    }
}
