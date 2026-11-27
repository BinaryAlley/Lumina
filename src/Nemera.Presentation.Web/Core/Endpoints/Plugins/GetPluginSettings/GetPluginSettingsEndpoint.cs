#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Authorization;
using Lumina.Presentation.Web.Common.DTO.Plugins;
using Lumina.Presentation.Web.Common.Requests.Plugins;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Plugins.GetPluginSettings;

/// <summary>
/// API endpoint for the <c>/{culture}/admin/manage-plugins/api-get-plugin-settings/{pluginId}</c> route.
/// </summary>
public class GetPluginSettingsEndpoint : BaseEndpoint<GetPluginSettingsRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPluginSettingsEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public GetPluginSettingsEndpoint(IApiHttpClient apiHttpClient)
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
        Routes(WebRoutes.Plugins.GET_PLUGIN_SETTINGS);
        DontAutoTag();
        Options(options => options.WithTags("Plugins"));
        Policies(AuthorizationPolicies.REQUIRE_ADMIN_ROLE);
    }

    /// <summary>
    /// Retrieves the settings of the plugin identified by the request.
    /// </summary>
    /// <param name="request">The request containing the unique identifier of the plugin whose settings are retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetPluginSettingsRequest request, CancellationToken cancellationToken)
    {
        PluginSettingsDto response = await _apiHttpClient.GetAsync<PluginSettingsDto>(ApiRoutes.Plugins.GET_PLUGIN_SETTINGS.Replace("{pluginId}", request.PluginId.ToString()), cancellationToken).ConfigureAwait(false);
        return JsonSuccess(response);
    }
}
