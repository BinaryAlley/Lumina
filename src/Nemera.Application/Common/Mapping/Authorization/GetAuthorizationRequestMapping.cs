#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.UsersManagement.Authorization.Queries.GetAuthorization;
using Lumina.Contracts.Requests.Authorization;
#endregion

namespace Lumina.Application.Common.Mapping.Authorization;

/// <summary>
/// Extension methods for converting <see cref="GetAuthorizationRequest"/>.
/// </summary>
public static class GetAuthorizationRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="GetAuthorizationQuery"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted query.</returns>
    public static GetAuthorizationQuery ToQuery(this GetAuthorizationRequest request)
    {
        return new GetAuthorizationQuery(
            request.UserId
        );
    }
}
