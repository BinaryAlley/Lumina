#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Themes;

/// <summary>
/// Data transfer object for the runtime configuration of the file system browser component.
/// </summary>
/// <param name="ClientBasePath">The base URL of the Web application used for file system API calls.</param>
/// <param name="Path">The initial path displayed by the file system browser.</param>
/// <param name="ViewMode">The initial view mode of the file system browser.</param>
/// <param name="IconSize">The initial icon size of the file system browser.</param>
[DebuggerDisplay("Path: {Path}, ViewMode: {ViewMode}")]
public sealed record ThemeFileSystemBrowserConfigurationDto(
    string ClientBasePath,
    string Path,
    string ViewMode,
    string IconSize
);
