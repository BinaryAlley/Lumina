#region ========================================================================= USING =====================================================================================
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Themes;

/// <summary>
/// Data transfer object for the full model available to a theme template while rendering a page.
/// </summary>
[DebuggerDisplay("Title: {Title}")]
public sealed class ThemePageDto
{
    /// <summary>
    /// Gets or sets the key of the page, used to select the template to render.
    /// </summary>
    public required string PageKey { get; init; }

    /// <summary>
    /// Gets or sets the name of the site displayed by the theme.
    /// </summary>
    public required string SiteName { get; init; }

    /// <summary>
    /// Gets or sets the title of the page.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Gets or sets the description of the page.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Gets or sets the eyebrow text shown above the page title.
    /// </summary>
    public required string Eyebrow { get; init; }

    /// <summary>
    /// Gets or sets the label of the primary call to action.
    /// </summary>
    public required string PrimaryActionLabel { get; init; }

    /// <summary>
    /// Gets or sets the URL of the primary call to action.
    /// </summary>
    public required string PrimaryActionUrl { get; init; }

    /// <summary>
    /// Gets or sets the label of the secondary call to action.
    /// </summary>
    public required string SecondaryActionLabel { get; init; }

    /// <summary>
    /// Gets or sets the URL of the secondary call to action.
    /// </summary>
    public required string SecondaryActionUrl { get; init; }

    /// <summary>
    /// Gets or sets the navigation entries rendered by the theme.
    /// </summary>
    public required IReadOnlyList<NavigationItemDto> Navigation { get; init; }

    /// <summary>
    /// Gets or sets the content entries rendered by the theme.
    /// </summary>
    public required IReadOnlyList<ContentItemDto> Items { get; init; }

    /// <summary>
    /// Gets or sets the statistics rendered by the theme.
    /// </summary>
    public required IReadOnlyList<StatItemDto> Stats { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether the page is a theme preview.
    /// </summary>
    public bool IsPreview { get; init; }

    /// <summary>
    /// Gets or sets the identifier of the theme rendering the page. Populated immediately before rendering.
    /// </summary>
    public string ThemeId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the theme rendering the page. Populated immediately before rendering.
    /// </summary>
    public string ThemeName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the version of the theme rendering the page. Populated immediately before rendering.
    /// </summary>
    public string ThemeVersion { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the base URL of the theme assets. Populated immediately before rendering.
    /// </summary>
    public string AssetBase { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current year. Populated immediately before rendering.
    /// </summary>
    public int CurrentYear { get; set; }
}
