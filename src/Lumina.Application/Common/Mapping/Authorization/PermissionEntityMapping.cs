#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Contracts.Responses.Authorization;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.Authorization;

/// <summary>
/// Extension methods for converting <see cref="PermissionEntity"/>.
/// </summary>
public static class PermissionEntityMapping
{
    /// <summary>
    /// Converts <paramref name="repositoryEntity"/> to <see cref="PermissionResponse"/>.
    /// </summary>
    /// <param name="repositoryEntity">The repository entity to be converted.</param>
    /// <returns>The converted response.</returns>
    public static PermissionResponse ToResponse(this PermissionEntity repositoryEntity)
    {
        return new PermissionResponse(
            repositoryEntity.Id,
            repositoryEntity.PermissionName
        );
    }

    /// <summary>
    /// Converts <paramref name="repositoryEntities"/> to a collection of <see cref="PermissionResponse"/>.
    /// </summary>
    /// <param name="repositoryEntities">The repository entities to be converted.</param>
    /// <returns>The converted responses.</returns>
    public static IEnumerable<PermissionResponse> ToResponses(this IEnumerable<PermissionEntity> repositoryEntities)
    {
        return repositoryEntities.Select(permission => permission.ToResponse());
    }
}
