#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Nemera.Presentation.Web.Common.Api;
using Nemera.Presentation.Web.Common.Authorization;
using Nemera.Presentation.Web.Common.DTO.Plugins;
using Nemera.Presentation.Web.Common.Routes;
using Nemera.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Web.Core.Endpoints.Plugins.GetPlugins;

/// <summary>
/// API endpoint for the <c>/{culture}/admin/manage-plugins/api-get-plugins</c> route.
/// </summary>
public class GetPluginsEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPluginsEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public GetPluginsEndpoint(IApiHttpClient apiHttpClient)
    {
        _apiHttpClient = apiHttpClient;
    }

    /// <summary>
    /// Configures the endpoint.
    /// </summary>
    public override void Configure()
    {
        base.Configure();
        Verbs(Http.GET);
        Routes(WebRoutes.Plugins.GET_PLUGINS);
        DontAutoTag();
        Options(options => options.WithTags("Plugins"));
        Policies(AuthorizationPolicies.REQUIRE_ADMIN_ROLE);
    }

    /// <summary>
    /// Retrieves the plugins.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        PluginDto[] response = await _apiHttpClient.GetAsync<PluginDto[]>(ApiRoutes.Plugins.GET_PLUGINS, cancellationToken).ConfigureAwait(false);
        return JsonSuccess(response);
    }
}
