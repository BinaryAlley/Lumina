#region ========================================================================= USING =====================================================================================
using Nemera.Application.Core.UsersManagement.Authorization.Queries.GetAuthorization;
using Nemera.Contracts.Requests.Authorization;
#endregion

namespace Nemera.Application.Common.Mapping.Authorization;

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
