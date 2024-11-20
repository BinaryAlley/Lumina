#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Controllers;

/// <summary>
/// Controller for home page.
/// </summary>
[Authorize]
[Route("")]  // randle root path - this is needed because app.UseCultureRedirect will redirect to the correct route
[Route("{culture}")] // culture-specific routes
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="HomeController"/> class.
    /// </summary>
    /// <param name="logger">Injected service for logging.</param>
    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Displays the view for the home page.
    /// </summary>
    [HttpGet("")]
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Displays the view for privacy.
    /// </summary>
    [HttpGet("privacy")]
    public IActionResult Privacy()
    {
        return View();
    }

    /// <summary>
    /// Displays the view for errors.
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [HttpGet("error")]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
