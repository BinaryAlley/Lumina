#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Authorization;
using Lumina.Presentation.Web.Common.DTO.Plugins;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Plugins.InstallPlugin;

/// <summary>
/// API endpoint for the <c>/{culture}/admin/manage-plugins/api-install-plugin</c> route.
/// </summary>
public class InstallPluginEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="InstallPluginEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public InstallPluginEndpoint(IApiHttpClient apiHttpClient)
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
        Routes(WebRoutes.Plugins.INSTALL_PLUGIN);
        DontAutoTag();
        Options(options => options.WithTags("Plugins"));
        Policies(AuthorizationPolicies.REQUIRE_ADMIN_ROLE);
        EnableAntiforgery();
    }

    /// <summary>
    /// Installs the plugin uploaded in the multipart form of the request.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        IFormFile? archive = HttpContext.Request.Form.Files.FirstOrDefault();
        if (archive is null)
            return Results.Problem(statusCode: StatusCodes.Status400BadRequest, detail: "The uploaded plugin archive is missing.");

        await using Stream archiveStream = archive.OpenReadStream();
        await _apiHttpClient.PostMultipartAsync<PluginDto>(ApiRoutes.Plugins.INSTALL_PLUGIN, archiveStream, archive.FileName, "archive", cancellationToken).ConfigureAwait(false);
        return JsonSuccess();
    }
}
