#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Attributes;
using Lumina.Presentation.Web.Common.Enums.Authorization;
using Lumina.Presentation.Web.Common.Models.Libraries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Controllers.Library.Management;

[Authorize]
[Route("{culture}/library/manage")]
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
        LibraryModel libraryModel = new();
        return View("/Views/Library/Management/Item.cshtml", libraryModel);
    }

    /// <summary>
    /// Displays the view for editing a media library.
    /// </summary>
    /// <param name="id">The id of the media library to edit.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [HttpGet("item/{id}")]
    public async Task<IActionResult> EditLibrary(Guid id, CancellationToken cancellationToken = default)
    {
        LibraryModel libraryModel = await _apiHttpClient.GetAsync<LibraryModel>($"libraries/{id}", cancellationToken).ConfigureAwait(false);
        return View("/Views/Library/Management/Item.cshtml", libraryModel);
    }

    /// <summary>
    /// Adds a new media library, or updates an existing one.
    /// </summary>
    /// <param name="data">The model containing the new library data.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [ValidateAntiForgeryToken]
    [HttpPost("api-item")]
    [RequirePermission(AuthorizationPermission.canCreateLibraries)]
    public async Task<IActionResult> SaveLibrary([FromBody] LibraryModel data, CancellationToken cancellationToken)
    {
        // call different API endpoints based on whether this is a new library or an existing one
        LibraryModel response = data.Id.HasValue
            ? await _apiHttpClient.PutAsync<LibraryModel, LibraryModel>($"libraries/{data.Id}", data, cancellationToken).ConfigureAwait(false)
            : await _apiHttpClient.PostAsync<LibraryModel, LibraryModel>("libraries", data, cancellationToken).ConfigureAwait(false);
        return Json(new { success = true, data = response });
    }
}
