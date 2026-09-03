#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.UsersManagement.Authentication.GetApiAccessToken;

/// <summary>
/// API endpoint for the <c>/{culture}/auth/api-access-token</c> route.
/// </summary>
public class GetApiAccessTokenEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    /// <summary>
    /// Configures the endpoint.
    /// </summary>
    public override void Configure()
    {
        base.Configure();
        Verbs(Http.GET);
        Routes(WebRoutes.Authentication.GET_API_ACCESS_TOKEN);
        DontAutoTag();
        Options(options => options.WithTags("Authentication"));
    }

    /// <summary>
    /// Gets the API access token of the current user, used to authenticate the real time SignalR connections of the current page.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override Task<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        // The response carries a credential, so it must never be cached by the browser or by any intermediary.
        HttpContext.Response.Headers.CacheControl = "no-store";
        HttpContext.Response.Headers.Pragma = "no-cache";

        if (User.Identity?.IsAuthenticated is not true)
            return Task.FromResult(Results.Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Unauthorized",
                detail: "You are not authorized"));

        string? token = User.FindFirst("Token")?.Value;
        if (string.IsNullOrEmpty(token))
            return Task.FromResult(Results.Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Unauthorized",
                detail: "The API access token of the current user is not available"));

        return Task.FromResult(JsonSuccess(new { token }));
    }
}
