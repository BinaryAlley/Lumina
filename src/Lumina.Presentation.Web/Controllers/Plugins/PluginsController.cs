#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Attributes;
using Lumina.Presentation.Web.Common.Enums.Plugins;
using Lumina.Presentation.Web.Common.Models.Common;
using Lumina.Presentation.Web.Common.Models.Plugins;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Controllers.Plugins;

/// <summary>
/// Controller for the plugin management related operations.
/// </summary>
[Authorize]
[RequireRole("Admin")]
[Route("{culture}/plugins")]
public class PluginsController : Controller
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginsController"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public PluginsController(IApiHttpClient apiHttpClient)
    {
        _apiHttpClient = apiHttpClient;
    }

    /// <summary>
    /// Displays the view for managing the plugins and their settings.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        PluginModel[] plugins = await _apiHttpClient.GetAsync<PluginModel[]>("plugins", cancellationToken).ConfigureAwait(false);

        // load the settings of the loaded plugins, so that the settings forms can be rendered on the server
        Dictionary<Guid, PluginSettingsModel> settingsByPluginId = [];
        foreach (PluginModel plugin in plugins)
            if (plugin.LoadStatus == PluginLoadStatus.Loaded)
                settingsByPluginId[plugin.Id] = await _apiHttpClient.GetAsync<PluginSettingsModel>($"plugins/{plugin.Id}/settings", cancellationToken).ConfigureAwait(false);

        ViewData["pluginSettings"] = settingsByPluginId;
        return View(plugins);
    }

    /// <summary>
    /// Gets the list of the detected plugins.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [HttpGet("api-get-plugins")]
    public async Task<IActionResult> GetPlugins(CancellationToken cancellationToken = default)
    {
        PluginModel[] response = await _apiHttpClient.GetAsync<PluginModel[]>("plugins/", cancellationToken).ConfigureAwait(false);
        return Json(new { success = true, data = response });
    }

    /// <summary>
    /// Gets the settings and their schema of the plugin identified by <paramref name="pluginId"/>.
    /// </summary>
    /// <param name="pluginId">The unique identifier of the plugin.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [HttpGet("api-get-plugin-settings/{pluginId}")]
    public async Task<IActionResult> GetPluginSettings(Guid pluginId, CancellationToken cancellationToken = default)
    {
        PluginSettingsModel response = await _apiHttpClient.GetAsync<PluginSettingsModel>($"plugins/{pluginId}/settings", cancellationToken).ConfigureAwait(false);
        return Json(new { success = true, data = response });
    }

    /// <summary>
    /// Updates the settings of the plugin identified by <paramref name="model"/>.
    /// </summary>
    /// <param name="model">The model containing the Id of the plugin and its settings.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [HttpPut("api-update-plugin-settings")]
    public async Task<IActionResult> UpdatePluginSettings([FromBody] UpdatePluginSettingsModel model, CancellationToken cancellationToken)
    {
        await _apiHttpClient.PutAsync<EmptyModel, UpdatePluginSettingsModel>($"plugins/{model.PluginId}/settings", model, cancellationToken).ConfigureAwait(false);
        return Json(new { success = true });
    }
}
