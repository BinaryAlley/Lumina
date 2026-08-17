#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Requests.UsersManagement;
using Lumina.Presentation.Web.Common.Responses.UsersManagement;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.UsersManagement.Authentication.Register;

/// <summary>
/// API endpoint for the <c>/{culture}/auth/api-register</c> route.
/// </summary>
public class RegisterEndpoint : BaseEndpoint<RegisterRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public RegisterEndpoint(IApiHttpClient apiHttpClient)
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
        Routes(WebRoutes.Authentication.REGISTER);
        DontAutoTag();
        Options(options => options.WithTags("Authentication"));
        AllowAnonymous();
        EnableAntiforgery();
    }

    /// <summary>
    /// Registers an account, or sets up the initial application admin account.
    /// </summary>
    /// <param name="request">The request containing the account credentials.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        // call different endpoints based on the view hidden field - registration for normal users, or initial application admin account setup
        string endpoint = request.RegistrationType == "Admin" ? ApiRoutes.Initialization.SETUP_APPLICATION : ApiRoutes.Authentication.REGISTER_ACCOUNT;
        // attempt API registration
        RegisterResponse response = await _apiHttpClient.PostAsync<RegisterResponse, RegisterRequest>(endpoint, request, cancellationToken).ConfigureAwait(false);
        return JsonSuccess(response);
    }
}
