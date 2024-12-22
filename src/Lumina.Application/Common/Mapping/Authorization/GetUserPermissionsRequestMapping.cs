#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.UsersManagement.Authorization.Queries.GetUserPermissions;
using Lumina.Contracts.Requests.Authorization;
#endregion

namespace Lumina.Application.Common.Mapping.Authorization;

/// <summary>
/// Extension methods for converting <see cref="GetUserPermissionsRequest"/>.
/// </summary>
public static class GetUserPermissionsRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="GetUserPermissionsQuery"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted query.</returns>
    public static GetUserPermissionsQuery ToQuery(this GetUserPermissionsRequest request)
    {
        return new GetUserPermissionsQuery(
            request.UserId
        );
    }
}
