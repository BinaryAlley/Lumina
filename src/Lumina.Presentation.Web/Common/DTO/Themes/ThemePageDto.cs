#region ========================================================================= USING =====================================================================================
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Themes;

/// <summary>
/// Data transfer object for the model available to a theme template while rendering a page.
/// </summary>
[DebuggerDisplay("Title: {Title}")]
public sealed class ThemePageDto
{
    /// <summary>
    /// Gets or sets the key of the page, used to select the template to render.
    /// </summary>
    public required string PageKey { get; init; }

    /// <summary>
    /// Gets or sets the title of the page.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Gets or sets the description of the page.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Gets or sets the resolved values exposed to the theme template and its scripts. Each page populates the entries its
    /// template needs, such as resolved URLs, identifiers and server paths, and the localized strings of the page under the <c>strings</c> key.
    /// </summary>
    public required IReadOnlyDictionary<string, object?> PageData { get; init; } = new Dictionary<string, object?>();

    /// <summary>
    /// Gets or sets the identifier of the script section of the page. Populated immediately before rendering.
    /// </summary>
    public string ScriptId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the identifier of the theme rendering the page. Populated immediately before rendering.
    /// </summary>
    public string ThemeId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the base URL of the theme assets. Populated immediately before rendering.
    /// </summary>
    public string AssetBase { get; set; } = string.Empty;
}
