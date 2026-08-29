#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Themes;
using Lumina.Presentation.Web.Common.Enums.FileSystem;
using Lumina.Presentation.Web.Common.Primitives;
using Lumina.Presentation.Web.Core.Themes;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace Lumina.Presentation.Web.Core.ViewComponents;

/// <summary>
/// View component for the file system browser.
/// </summary>
public class FileSystemBrowserViewComponent : ViewComponent
{
    private readonly ThemeFileSystemBrowserRenderer _renderer;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileSystemBrowserViewComponent"/> class.
    /// </summary>
    /// <param name="renderer">Injected renderer of the themed file system browser component.</param>
    public FileSystemBrowserViewComponent(ThemeFileSystemBrowserRenderer renderer)
    {
        _renderer = renderer;
    }

    /// <summary>
    /// Invokes the <see cref="FileSystemBrowser"/> view component, rendering the theme template of the file system browser when available.
    /// </summary>
    /// <returns>An <see cref="IViewComponentResult"/> that renders the themed component, or the application fallback view when the theme template is unavailable.</returns>
    public async Task<IViewComponentResult> InvokeAsync()
    {
        ThemeFileSystemBrowserConfigurationDto configuration = new(
            ClientBasePath: "http://localhost:5012/", // TODO: take from appsettings or environment
            Path: RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? @"/" : @"C:\Users\",
            ViewMode: FileSystemViewMode.List.ToString().ToLower(),
            IconSize: FileSystemIconSize.Large.ToString().ToLower());

        Result<string> themedResult = await _renderer.RenderAsync(configuration, CancellationToken.None).ConfigureAwait(false);
        if (themedResult.IsSuccess)
            return View("Themed", themedResult.Value);

        return View(new
        {
            Path = configuration.Path,
            ClientBasePath = configuration.ClientBasePath,
            ViewMode = configuration.ViewMode,
            IconSize = configuration.IconSize
        });
    }
}
