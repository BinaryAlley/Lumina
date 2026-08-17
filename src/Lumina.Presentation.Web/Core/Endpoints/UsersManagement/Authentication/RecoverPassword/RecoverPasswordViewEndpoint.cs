#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Authorization;
using Lumina.Presentation.Web.Common.Requests.UsersManagement;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Common.Services;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.UsersManagement.Authentication.RecoverPassword;

/// <summary>
/// API endpoint for the <c>/{culture}/auth/recover-password</c> route.
/// </summary>
public class RecoverPasswordViewEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    private readonly IUrlService _urlService;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecoverPasswordViewEndpoint"/> class.
    /// </summary>
    /// <param name="urlService">Injected service for generating URLs from action and controller names, with localization.</param>
    public RecoverPasswordViewEndpoint(IUrlService urlService)
    {
        _urlService = urlService;
    }

    /// <summary>
    /// Configures the endpoint.
    /// </summary>
    public override void Configure()
    {
        base.Configure();
        Verbs(Http.GET);
        Routes(WebRoutes.Authentication.RECOVER_PASSWORD_VIEW);
        DontAutoTag();
        Options(options => options.WithTags("Authentication"));
        AllowAnonymous();
        PreProcessor<InitializationCheckPreProcessor<EmptyRequest>>();
    }

    /// <summary>
    /// Displays the account password recovery view.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override Task<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        // if the user is already logged in, redirect them to the home page
        if (User?.Identity?.IsAuthenticated == true)
            return Task.FromResult(Results.Redirect(_urlService.GetAbsoluteUrl(WebRoutes.Home.INDEX_CULTURED)!));
        RecoverPasswordRequest recoverPasswordRequest = new(Username: null, TotpCode: null);
        return Task.FromResult(View("/Core/Views/Auth/RecoverPassword.cshtml", recoverPasswordRequest));
    }
}
