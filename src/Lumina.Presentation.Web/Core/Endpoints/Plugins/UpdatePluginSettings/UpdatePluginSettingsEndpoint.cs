#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Authorization;
using Lumina.Presentation.Web.Common.Requests.Plugins;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Plugins.UpdatePluginSettings;

/// <summary>
/// API endpoint for the <c>/{culture}/admin/manage-plugins/api-update-plugin-settings</c> route.
/// </summary>
public class UpdatePluginSettingsEndpoint : BaseEndpoint<UpdatePluginSettingsRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePluginSettingsEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public UpdatePluginSettingsEndpoint(IApiHttpClient apiHttpClient)
    {
        _apiHttpClient = apiHttpClient;
    }

    /// <summary>
    /// Configures the endpoint.
    /// </summary>
    public override void Configure()
    {
        base.Configure();
        Verbs(Http.PUT);
        Routes(WebRoutes.Plugins.UPDATE_PLUGIN_SETTINGS);
        DontAutoTag();
        Options(options => options.WithTags("Plugins"));
        Policies(AuthorizationPolicies.REQUIRE_ADMIN_ROLE);
        EnableAntiforgery();
    }

    /// <summary>
    /// Updates the settings of the plugin identified by the request.
    /// </summary>
    /// <param name="request">The request containing the updated settings of the plugin.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(UpdatePluginSettingsRequest request, CancellationToken cancellationToken)
    {
        await _apiHttpClient.PutAsync<Web.Common.Requests.Common.EmptyRequest, UpdatePluginSettingsRequest>(ApiRoutes.Plugins.UPDATE_PLUGIN_SETTINGS.Replace("{pluginId}", request.PluginId.ToString()), request, cancellationToken).ConfigureAwait(false);
        return JsonSuccess();
    }
}
