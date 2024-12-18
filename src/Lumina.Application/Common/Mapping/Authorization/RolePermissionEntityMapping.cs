#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Contracts.DTO.Authentication;
using Lumina.Contracts.Responses.Authorization;
#endregion

namespace Lumina.Application.Common.Mapping.Authorization;

/// <summary>
/// Extension methods for converting <see cref="RolePermissionEntity"/>.
/// </summary>
public static class RolePermissionEntityMapping
{
    /// <summary>
    /// Converts <paramref name="repositoryEntity"/> to <see cref="RolePermissionsResponse"/>.
    /// </summary>
    /// <param name="repositoryEntity">The repository entity to be converted.</param>
    /// <returns>The converted response.</returns>
    public static PermissionDto ToResponse(this RolePermissionEntity repositoryEntity)
    {
        return new PermissionDto(
            repositoryEntity.PermissionId,
            repositoryEntity.Permission.PermissionName
        );
    }
}
