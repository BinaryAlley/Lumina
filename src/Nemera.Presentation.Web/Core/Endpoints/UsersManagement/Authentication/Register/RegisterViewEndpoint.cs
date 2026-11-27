#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Nemera.Presentation.Web.Common.Authorization;
using Nemera.Presentation.Web.Common.Http;
using Nemera.Presentation.Web.Common.Requests.UsersManagement;
using Nemera.Presentation.Web.Common.Routes;
using Nemera.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Web.Core.Endpoints.UsersManagement.Authentication.Register;

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
        // Check if this is the initial super admin setup.
        string? isPendingSuperAdminSetup = HttpContext.Session.GetString(HttpContextItemKeys.PENDING_SUPER_ADMIN_SETUP);
        string registrationType = isPendingSuperAdminSetup == "true" ? "Admin" : "User";
        Dictionary<string, object?> viewData = new() { ["RegistrationType"] = registrationType };
        RegisterRequest registerRequest = new(Username: null, Password: null, PasswordConfirm: null, RegistrationType: registrationType);
        return Task.FromResult(View("/Core/Views/Auth/Register.cshtml", registerRequest, viewData));
    }
}
