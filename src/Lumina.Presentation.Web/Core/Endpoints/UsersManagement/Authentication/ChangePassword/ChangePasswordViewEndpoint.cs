#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Requests.UsersManagement;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.UsersManagement.Authentication.ChangePassword;

/// <summary>
/// API endpoint for the <c>/{culture}/auth/change-password</c> route.
/// </summary>
public class ChangePasswordViewEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    /// <summary>
    /// Configures the endpoint.
    /// </summary>
    public override void Configure()
    {
        base.Configure();
        Verbs(Http.GET);
        Routes(WebRoutes.Authentication.CHANGE_PASSWORD_VIEW);
        DontAutoTag();
        Options(options => options.WithTags("Authentication"));
    }

    /// <summary>
    /// Displays the account password change view.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override Task<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        ChangePasswordRequest changePasswordRequest = new(Username: null, CurrentPassword: null, NewPassword: null, NewPasswordConfirm: null);
        return Task.FromResult(View("/Core/Views/Auth/ChangePassword.cshtml", changePasswordRequest));
    }
}
