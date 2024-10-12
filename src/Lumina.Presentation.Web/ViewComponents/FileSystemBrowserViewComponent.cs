#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Enums.FileSystem;
using Lumina.Presentation.Web.Common.Models.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Runtime.InteropServices;

#endregion

namespace Lumina.Presentation.Web.ViewComponents;

/// <summary>
/// View component for the file system browser.
/// </summary>
public class FileSystemBrowserViewComponent : ViewComponent
{
    private readonly ServerConfigurationModel _serverConfigurationModel;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileSystemBrowserViewComponent"/> class.
    /// </summary>
    /// <param name="serverConfigurationModelOptions">Injected service for retrieving <see cref="ServerConfigurationModel"/>.</param>
    public FileSystemBrowserViewComponent(IOptions<ServerConfigurationModel> serverConfigurationModelOptions)
    {
        _serverConfigurationModel = serverConfigurationModelOptions.Value;
    }

    /// <summary>
    /// Invokes the <see cref="FileSystemBrowser"/> view component.
    /// </summary>
    /// <returns>
    /// An <see cref="IViewComponentResult"/> that renders the view with file system browsing configuration.
    /// </returns>
    public IViewComponentResult Invoke()
    {
        return View(new
        { 
            Path = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? @"/" : @"C:\",
            ServerBasePath = $"{_serverConfigurationModel.BaseAddress}:{_serverConfigurationModel.Port}/api/v{_serverConfigurationModel.ApiVersion}/",
            ClientBasePath = "http://localhost:5012/", // TODO: take from appsettings or environment
            ViewMode = FileSystemViewMode.List,
            IconSize = FileSystemIconSize.Large
        });
    }
}
