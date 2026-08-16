#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Models.Common;
using Lumina.Presentation.Web.Common.Models.UsersManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Controllers.Tools;

/// <summary>
/// Controller for the settings of the current user.
/// </summary>
[Authorize]
[Route("{culture}/tools/settings")]
public class SettingsController : Controller
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsController"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public SettingsController(IApiHttpClient apiHttpClient)
    {
        _apiHttpClient = apiHttpClient;
    }

    /// <summary>
    /// Displays the view for editing the settings of the current user.
    /// </summary>
    [HttpGet("")]
    public IActionResult Index()
    {
        UserSettingsModel settings = new();
        return View("/Views/Tools/Settings.cshtml", settings);
    }

    /// <summary>
    /// Gets the settings of the current user.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [HttpGet("api-get-user-settings")]
    public async Task<IActionResult> GetUserSettings(CancellationToken cancellationToken = default)
    {
        UserSettingsModel response = await _apiHttpClient.GetAsync<UserSettingsModel>("users/me/settings", cancellationToken).ConfigureAwait(false);
        return Json(new { success = true, data = response });
    }

    /// <summary>
    /// Updates the settings of the current user.
    /// </summary>
    /// <param name="data">The model containing the new settings of the current user.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [ValidateAntiForgeryToken]
    [HttpPost("api-update-user-settings")]
    public async Task<IActionResult> UpdateUserSettings([FromBody] UserSettingsModel data, CancellationToken cancellationToken)
    {
        await _apiHttpClient.PutAsync<EmptyModel, UserSettingsModel>("users/me/settings", data, cancellationToken).ConfigureAwait(false);
        return Json(new { success = true, data = new { isUpdated = true } });
    }
}
