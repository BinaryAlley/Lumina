#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Attributes;
using Lumina.Presentation.Web.Common.Enums.Authorization;
using Lumina.Presentation.Web.Common.Models.Common;
using Lumina.Presentation.Web.Common.Models.Libraries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Controllers.Library.Management;

/// <summary>
/// Controller for media libraries related operations.
/// </summary>
[Authorize]
[Route("{culture}/libraries/manage")]
public class LibraryManagementController : Controller
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryManagementController"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public LibraryManagementController(IApiHttpClient apiHttpClient)
    {
        _apiHttpClient = apiHttpClient;
    }

    /// <summary>
    /// Displays the view for displaying the list of media libraries.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [HttpGet("")]
    public IActionResult Index()
    {
        return View("/Views/Library/Management/Index.cshtml");
    }

    /// <summary>
    /// Displays the view for adding a media library.
    /// </summary>
    [HttpGet("item")]
    public IActionResult AddLibrary()
    {
        LibraryModel library = new();
        return View("/Views/Library/Management/Item.cshtml", library);
    }

    /// <summary>
    /// Displays the view for editing a media library.
    /// </summary>
    /// <param name="id">The id of the media library to edit.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [HttpGet("item/{id}")]
    public async Task<IActionResult> EditLibrary(Guid id, CancellationToken cancellationToken = default)
    {
        LibraryModel response = await _apiHttpClient.GetAsync<LibraryModel>($"libraries/{id}", cancellationToken).ConfigureAwait(false);
        return View("/Views/Library/Management/Item.cshtml", response);
    }

    /// <summary>
    /// Gets the list of media libraries.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [HttpGet("api-get-libraries")]
    public async Task<IActionResult> GetLibraries(CancellationToken cancellationToken = default)
    {
        LibraryModel[] response = await _apiHttpClient.GetAsync<LibraryModel[]>($"libraries/", cancellationToken).ConfigureAwait(false);
        return Json(new { success = true, data = response });
    }

    /// <summary>
    /// Gets the list of enabled media libraries.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [HttpGet("api-get-enabled-libraries")]
    public async Task<IActionResult> GetEnabledLibraries(CancellationToken cancellationToken = default)
    {
        LibraryModel[] response = await _apiHttpClient.GetAsync<LibraryModel[]>($"libraries/enabled/", cancellationToken).ConfigureAwait(false);
        return Json(new { success = true, data = response });
    }

    /// <summary>
    /// Adds a new media library, or updates an existing one.
    /// </summary>
    /// <param name="data">The model containing the new library data.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [ValidateAntiForgeryToken]
    [HttpPost("api-item")]
    [RequirePermission(AuthorizationPermission.CanCreateLibraries)]
    public async Task<IActionResult> SaveLibrary([FromBody] LibraryModel data, CancellationToken cancellationToken)
    {
        // call different API endpoints based on whether this is a new library or an existing one
        LibraryModel response = data.Id.HasValue
            ? await _apiHttpClient.PutAsync<LibraryModel, LibraryModel>($"libraries/{data.Id}", data, cancellationToken).ConfigureAwait(false)
            : await _apiHttpClient.PostAsync<LibraryModel, LibraryModel>("libraries", data, cancellationToken).ConfigureAwait(false);
        return Json(new { success = true, data = response });
    }

    /// <summary>
    /// Deletes a media library.
    /// </summary>
    /// <param name="id">The id of the media library to delete.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [HttpDelete("api-item/{id}")]
    public async Task<IActionResult> DeleteLibrary(Guid id, CancellationToken cancellationToken = default)
    {
        await _apiHttpClient.DeleteAsync($"libraries/{id}", cancellationToken).ConfigureAwait(false);
        return Json(new { success = true });
    }

    /// <summary>
    /// Gets the list of ongoing media library scans.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [HttpGet("api-get-running-library-scans")]
    public async Task<IActionResult> GetRunningLibraryScans(CancellationToken cancellationToken = default)
    {
        LibraryScanProgressModel[] response = await _apiHttpClient.GetAsync<LibraryScanProgressModel[]>($"libraries/scans/running/", cancellationToken).ConfigureAwait(false);
        return Json(new { success = true, data = response });
    }

    /// <summary>
    /// Initiates the scan of all media libraries.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [HttpPost("api-scan-libraries")]
    public async Task<IActionResult> ScanAllLibraries(CancellationToken cancellationToken = default)
    {
        ScanLibraryModel[] response = await _apiHttpClient.PostAsync<ScanLibraryModel[], EmptyModel>($"libraries/scans", new EmptyModel(), cancellationToken).ConfigureAwait(false);
        return Json(new { success = true, data = response });
    }

    /// <summary>
    /// Initiates the scan of a media library.
    /// </summary>
    /// <param name="id">The id of the media library whose scan is started.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [HttpPost("api-scan-library/{id}")]
    public async Task<IActionResult> ScanLibrary(Guid id, CancellationToken cancellationToken = default)
    {
        ScanLibraryModel response = await _apiHttpClient.PostAsync<ScanLibraryModel, EmptyModel>($"libraries/{id}/scans", new EmptyModel(), cancellationToken).ConfigureAwait(false);
        return Json(new { success = true, data = response });
    }

    /// <summary>
    /// Cancels the previously started scan of all media libraries.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [HttpPost("api-cancel-libraries-scan")]
    public async Task<IActionResult> CancelLibrariesScan(CancellationToken cancellationToken = default)
    {
        await _apiHttpClient.PostAsync<EmptyModel, EmptyModel>($"libraries/scans/cancel", new EmptyModel(), cancellationToken).ConfigureAwait(false);
        return Json(new { success = true });
    }

    /// <summary>
    /// Cancels the previously started scan of a media library.
    /// </summary>
    /// <param name="libraryId">The id of the media library whose scan is cancelled.</param>
    /// <param name="scanId">The id of the scan to cancel.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [HttpPost("{libraryId:guid}/api-cancel-library-scan/{scanId:guid}")]
    public async Task<IActionResult> CancelLibraryScan(Guid libraryId, Guid scanId, CancellationToken cancellationToken = default)
    {
        await _apiHttpClient.PostAsync<EmptyModel, EmptyModel>($"libraries/{libraryId}/scans/{scanId}/cancel", new EmptyModel(), cancellationToken).ConfigureAwait(false);
        return Json(new { success = true });
    }
}
