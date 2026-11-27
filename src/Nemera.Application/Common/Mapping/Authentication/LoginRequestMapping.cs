#region ========================================================================= USING =====================================================================================
using Nemera.Application.Core.UsersManagement.Authentication.Queries.LoginUser;
using Nemera.Contracts.Requests.Authentication;
#endregion

namespace Nemera.Application.Common.Mapping.Authentication;

/// <summary>
/// Extension methods for converting <see cref="LoginRequest"/>.
/// </summary>
public static class LoginRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="LoginUserQuery"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted query.</returns>
    public static LoginUserQuery ToQuery(this LoginRequest request)
    {
        return new LoginUserQuery(
            request.Username,
            request.Password,
            request.TotpCode
        );
    }
}
