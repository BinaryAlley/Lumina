#region ========================================================================= USING =====================================================================================
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Themes;

/// <summary>
/// Data transfer object for the model available to the themed file system browser template.
/// </summary>
/// <param name="AppBase">The absolute base URL of the Web application, used to reference the application scripts from the theme template.</param>
/// <param name="AssetBase">The base URL of the theme assets, used by the theme template to reference its own assets.</param>
/// <param name="IconBaseUrl">The base URL of the file type icons of the theme.</param>
/// <param name="FileIconsUrl">The URL of the theme file icons mapping, mapping file extensions to icon paths.</param>
/// <param name="TreeNodeTemplate">The raw template source of the tree view node, rendered client side for each node.</param>
/// <param name="ExplorerItemTemplate">The raw template source of the explorer item, rendered client side for each file system entry.</param>
/// <param name="PathSegmentTemplate">The raw template source of the address bar path segment, rendered client side for each segment.</param>
/// <param name="Strings">The localized strings of the file system browser, keyed by resource name.</param>
[DebuggerDisplay("AssetBase: {AssetBase}")]
public sealed record ThemeFileSystemBrowserDto(
    string AppBase,
    string AssetBase,
    string IconBaseUrl,
    string FileIconsUrl,
    string TreeNodeTemplate,
    string ExplorerItemTemplate,
    string PathSegmentTemplate,
    IReadOnlyDictionary<string, object?> Strings
);
