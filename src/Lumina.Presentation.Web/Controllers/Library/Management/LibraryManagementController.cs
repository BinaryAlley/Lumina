#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Enums.Library;
using Lumina.Presentation.Web.Common.Models.Libraries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using System;
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
    public IActionResult EditLibrary(Guid? id = null)
    {
        LibraryModel libraryModel = new()
        {
            Id = id,
            LibraryType = LibraryType.Book,
            Paths = ["a", "b", "c"],
            Title = ""
        };
        return View("/Views/Library/Management/Item.cshtml", libraryModel);
    }

    [HttpPost("item")]
    public async Task<IActionResult> SaveLibraryAsync([FromBody] LibraryModel model)
    {
        LibraryModel response = model.Id.HasValue
            ? await _apiHttpClient.PutAsync<LibraryModel, LibraryModel>($"libraries/{model.Id}", model)
            : await _apiHttpClient.PostAsync<LibraryModel, LibraryModel>("libraries", model);
        return Json(new { success = true });
    }
}
