#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Authorization;
using Lumina.Presentation.Web.Common.Http;
using Lumina.Presentation.Web.Common.Requests.UsersManagement;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.UsersManagement.Authentication.Register;

/// <summary>
/// API endpoint for the <c>/{culture}/auth/register</c> route.
/// </summary>
public class RegisterViewEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    /// <summary>
    /// Configures the endpoint.
    /// </summary>
    public override void Configure()
    {
        base.Configure();
        Verbs(Http.GET);
        Routes(WebRoutes.Authentication.REGISTER_VIEW);
        DontAutoTag();
        Options(options => options.WithTags("Authentication"));
        AllowAnonymous();
        PreProcessor<InitializationCheckPreProcessor<EmptyRequest>>();
    }

    /// <summary>
    /// Displays the account registration view.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override Task<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        // check if this is the initial super admin setup
        string? isPendingSuperAdminSetup = HttpContext.Session.GetString(HttpContextItemKeys.PENDING_SUPER_ADMIN_SETUP);
        string registrationType = isPendingSuperAdminSetup == "true" ? "Admin" : "User";
        Dictionary<string, object?> viewData = new() { ["RegistrationType"] = registrationType };
        RegisterRequest registerRequest = new(Username: null, Password: null, PasswordConfirm: null, RegistrationType: registrationType);
        return Task.FromResult(View("/Core/Views/Auth/Register.cshtml", registerRequest, viewData));
    }
}
