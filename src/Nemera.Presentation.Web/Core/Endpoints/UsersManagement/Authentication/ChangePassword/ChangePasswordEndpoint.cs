#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Nemera.Presentation.Web.Common.Api;
using Nemera.Presentation.Web.Common.Requests.UsersManagement;
using Nemera.Presentation.Web.Common.Responses.UsersManagement;
using Nemera.Presentation.Web.Common.Routes;
using Nemera.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Web.Core.Endpoints.UsersManagement.Authentication.ChangePassword;

/// <summary>
/// API endpoint for the <c>/{culture}/auth/api-change-password</c> route.
/// </summary>
public class ChangePasswordEndpoint : BaseEndpoint<ChangePasswordRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChangePasswordEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public ChangePasswordEndpoint(IApiHttpClient apiHttpClient)
    {
        _apiHttpClient = apiHttpClient;
    }

    /// <summary>
    /// Configures the endpoint.
    /// </summary>
    public override void Configure()
    {
        base.Configure();
        Verbs(Http.POST);
        Routes(WebRoutes.Authentication.CHANGE_PASSWORD);
        DontAutoTag();
        Options(options => options.WithTags("Authentication"));
        EnableAntiforgery();
    }

    /// <summary>
    /// Changes the password of the currently logged in account.
    /// </summary>
    /// <param name="request">The request containing the account credentials.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        // Assign the currently logged in user as the user for which to change the password.
        ChangePasswordRequest changePasswordRequest = request with { Username = User?.Identity?.Name };
        ChangePasswordResponse response = await _apiHttpClient.PostAsync<ChangePasswordResponse, ChangePasswordRequest>(ApiRoutes.Authentication.CHANGE_PASSWORD, changePasswordRequest, cancellationToken).ConfigureAwait(false);
        return JsonSuccess(response);
    }
}
