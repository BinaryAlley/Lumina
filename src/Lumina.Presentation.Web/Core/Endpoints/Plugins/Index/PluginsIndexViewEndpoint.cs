#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Authorization;
using Lumina.Presentation.Web.Common.DTO.Plugins;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Plugins.Index;

/// <summary>
/// API endpoint for the <c>/{culture}/admin/manage-plugins</c> route.
/// </summary>
public class PluginsIndexViewEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginsIndexViewEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public PluginsIndexViewEndpoint(IApiHttpClient apiHttpClient)
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
        Routes(WebRoutes.Plugins.INDEX);
        DontAutoTag();
        Options(options => options.WithTags("Plugins"));
        Policies(AuthorizationPolicies.REQUIRE_ADMIN_ROLE);
    }

    /// <summary>
    /// Displays the plugins index view.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        PluginDto[] plugins = await _apiHttpClient.GetAsync<PluginDto[]>(ApiRoutes.Plugins.GET_PLUGINS, cancellationToken).ConfigureAwait(false);
        Dictionary<Guid, PluginSettingsDto> settingsByPluginId = [];
        foreach (PluginDto plugin in plugins)
            if (plugin.LoadStatus == Lumina.Presentation.Web.Common.Enums.Plugins.PluginLoadStatus.Loaded)
                settingsByPluginId[plugin.Id] = await _apiHttpClient.GetAsync<PluginSettingsDto>(ApiRoutes.Plugins.GET_PLUGIN_SETTINGS.Replace("{pluginId}", plugin.Id.ToString()), cancellationToken).ConfigureAwait(false);
        Dictionary<string, object?> viewData = new() { ["pluginSettings"] = settingsByPluginId };
        return View("/Core/Views/Admin/Plugins.cshtml", plugins, viewData);
    }
}
