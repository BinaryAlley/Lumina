#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.UsersManagement.Authentication.AccessDenied;

/// <summary>
/// API endpoint for the <c>/{culture}/auth/access-denied</c> route.
/// </summary>
public class AccessDeniedViewEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    /// <summary>
    /// Configures the endpoint.
    /// </summary>
    public override void Configure()
    {
        base.Configure();
        Verbs(Http.GET);
        Routes(WebRoutes.Authentication.ACCESS_DENIED_VIEW);
        DontAutoTag();
        Options(options => options.WithTags("Authentication"));
        AllowAnonymous();
    }

    /// <summary>
    /// Displays the access denied view.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override Task<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(View("/Core/Views/Auth/AccessDenied.cshtml"));
    }
}
