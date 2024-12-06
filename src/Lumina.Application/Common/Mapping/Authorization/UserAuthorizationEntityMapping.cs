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
    /// Converts <paramref name="infrastructureEntity"/> to <see cref="GetAuthorizationResponse"/>.
    /// </summary>
    /// <param name="infrastructureEntity">The infrastructure entity to be converted.</param>
    /// <returns>The converted response.</returns>
    public static GetAuthorizationResponse ToResponse(this UserAuthorizationEntity infrastructureEntity)
    {
        return new GetAuthorizationResponse(
            infrastructureEntity.UserId,
            infrastructureEntity.Roles,
            infrastructureEntity.Permissions
        );
    }
}
