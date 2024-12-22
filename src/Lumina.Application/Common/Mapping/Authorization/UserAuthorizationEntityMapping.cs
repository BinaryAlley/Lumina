#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Contracts.Responses.Authorization;
#endregion

namespace Lumina.Application.Common.Mapping.Authorization;

/// <summary>
/// Extension methods for converting <see cref="UserAuthorizationEntity"/>.
/// </summary>
public static class UserAuthorizationEntityMapping
{
    /// <summary>
    /// Converts <paramref name="repositoryEntity"/> to <see cref="AuthorizationResponse"/>.
    /// </summary>
    /// <param name="repositoryEntity">The infrastructure entity to be converted.</param>
    /// <returns>The converted response.</returns>
    public static AuthorizationResponse ToResponse(this UserAuthorizationEntity repositoryEntity)
    {
        return new AuthorizationResponse(
            repositoryEntity.UserId,
            repositoryEntity.Role,
            repositoryEntity.Permissions
        );
    }
}
