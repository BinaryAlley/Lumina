#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Themes;

/// <summary>
/// Data transfer object for the model available to the themed layout template while rendering the shell of a page.
/// </summary>
/// <param name="Title">The title of the page, rendered by the theme.</param>
/// <param name="AssetBase">The base URL of the theme assets.</param>
/// <param name="AppHead">The application head links, such as its global stylesheets.</param>
/// <param name="Nav">The rendered navigation menu of the page.</param>
/// <param name="Content">The rendered content section of the page.</param>
/// <param name="AudioPlayer">The rendered audio player, when the current user is authenticated.</param>
/// <param name="AppScripts">The application global scripts, such as its client libraries and configuration.</param>
/// <param name="Scripts">The rendered script section of the page.</param>
/// <param name="MainStyle">The inline style of the main content area, used to fill the viewport when the user is not authenticated.</param>
[DebuggerDisplay("Title: {Title}")]
public sealed record ThemeLayoutModelDto(
    string Title,
    string AssetBase,
    string AppHead,
    string Nav,
    string Content,
    string AudioPlayer,
    string AppScripts,
    string Scripts,
    string MainStyle
);
