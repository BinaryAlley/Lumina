#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Enums.Library;
using Lumina.Presentation.Web.Common.Models.Libraries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Controllers.Library.Management;

[Authorize]
[Route("/library/manage/")]
public class LibraryManagementController : Controller
{
    private readonly IApiHttpClient _apiHttpClient;

    public LibraryManagementController(IApiHttpClient apiHttpClient)
    {
        _apiHttpClient = apiHttpClient;
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        return View("/Views/Library/Management/Index.cshtml");
    }

    [HttpGet("item/{id?}")]
    public async Task<IActionResult> EditLibrary(Guid? id = null, CancellationToken cancellationToken = default)
    {
        LibraryModel libraryModel = new();
        if (id is not null)
            libraryModel = await _apiHttpClient.GetAsync<LibraryModel>($"libraries/{id}", cancellationToken);
        return View("/Views/Library/Management/Item.cshtml", libraryModel);
    }

    /// <summary>
    /// Adds a new media library, or updates an existing one.
    /// </summary>
    /// <param name="data">The model containing the new library data.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [ValidateAntiForgeryToken]
    [HttpPost("item")]
    public async Task<IActionResult> SaveLibrary([FromBody] LibraryModel data, CancellationToken cancellationToken)
    {
        // call different API endpoints based on whether this is a new library or an existing one
        LibraryModel response = data.Id.HasValue
            ? await _apiHttpClient.PutAsync<LibraryModel, LibraryModel>($"libraries/{data.Id}", data, cancellationToken)
            : await _apiHttpClient.PostAsync<LibraryModel, LibraryModel>("libraries", data, cancellationToken);
        return Json(new { success = true, data = response });
    }
}
