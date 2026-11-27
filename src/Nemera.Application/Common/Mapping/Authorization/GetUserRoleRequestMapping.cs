#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.UsersManagement.Authorization.Queries.GetUserRole;
using Lumina.Contracts.Requests.Authorization;
#endregion

namespace Lumina.Application.Common.Mapping.Authorization;

/// <summary>
/// Extension methods for converting <see cref="GetUserRoleRequest"/>.
/// </summary>
public static class GetUserRoleRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="GetUserRoleQuery"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted query.</returns>
    public static GetUserRoleQuery ToQuery(this GetUserRoleRequest request)
    {
        return new GetUserRoleQuery(
            request.UserId
        );
    }
}
