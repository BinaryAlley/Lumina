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

namespace Nemera.Presentation.Web.Core.Endpoints.UsersManagement.Authentication.RecoverPassword;

/// <summary>
/// API endpoint for the <c>/{culture}/auth/api-recover-password</c> route.
/// </summary>
public class RecoverPasswordEndpoint : BaseEndpoint<RecoverPasswordRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecoverPasswordEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public RecoverPasswordEndpoint(IApiHttpClient apiHttpClient)
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
        Routes(WebRoutes.Authentication.RECOVER_PASSWORD);
        DontAutoTag();
        Options(options => options.WithTags("Authentication"));
        AllowAnonymous();
        EnableAntiforgery();
    }

    /// <summary>
    /// Recovers the password of an account.
    /// </summary>
    /// <param name="request">The request containing the account credentials.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(RecoverPasswordRequest request, CancellationToken cancellationToken)
    {
        RecoverPasswordResponse response = await _apiHttpClient.PostAsync<RecoverPasswordResponse, RecoverPasswordRequest>(ApiRoutes.Authentication.RECOVER_PASSWORD, request, cancellationToken).ConfigureAwait(false);
        return JsonSuccess(response);
    }
}
