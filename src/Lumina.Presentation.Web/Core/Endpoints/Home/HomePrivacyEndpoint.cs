#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Home;

/// <summary>
/// API endpoint for the <c>/{culture}/privacy</c> route.
/// </summary>
public class HomePrivacyEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    /// <summary>
    /// Configures the endpoint.
    /// </summary>
    public override void Configure()
    {
        base.Configure();
        Verbs(Http.GET);
        Routes(WebRoutes.Home.PRIVACY);
        DontAutoTag();
        Options(options => options.WithTags("Home"));
    }

    /// <summary>
    /// Displays the privacy page.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override Task<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(View("/Core/Views/Home/Privacy.cshtml"));
    }
}
