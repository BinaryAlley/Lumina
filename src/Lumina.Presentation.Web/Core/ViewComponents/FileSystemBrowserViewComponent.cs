#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Configuration;
using Lumina.Presentation.Web.Common.Enums.FileSystem;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Runtime.InteropServices;

#endregion

namespace Lumina.Presentation.Web.Core.ViewComponents;

/// <summary>
/// View component for the file system browser.
/// </summary>
public class FileSystemBrowserViewComponent : ViewComponent
{
    private readonly ServerConfigurationDto _serverConfiguration;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileSystemBrowserViewComponent"/> class.
    /// </summary>
    /// <param name="serverConfigurationOptions">Injected service for retrieving <see cref="ServerConfigurationDto"/>.</param>
    public FileSystemBrowserViewComponent(IOptions<ServerConfigurationDto> serverConfigurationOptions)
    {
        _serverConfiguration = serverConfigurationOptions.Value;
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
            Path = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? @"/" : @"C:\Users\",
            ServerBasePath = $"{_serverConfiguration.BaseAddress}:{_serverConfiguration.Port}/api/v{_serverConfiguration.ApiVersion}/",
            ClientBasePath = "http://localhost:5012/", // TODO: take from appsettings or environment
            ViewMode = FileSystemViewMode.List,
            IconSize = FileSystemIconSize.Large
        });
    }
}
