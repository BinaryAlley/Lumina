#region ========================================================================= USING =====================================================================================
using System;
#endregion

namespace Lumina.Plugins.OpenLibrary.Common.Models.DTO.Settings;

/// <summary>
/// Data transfer object for the settings that configure the Open Library metadata plugin.
/// </summary>
internal sealed class OpenLibrarySettingsDto
{
    /// <summary>
    /// Gets or sets the user agent sent with every request to the Open Library API.
    /// </summary>
    public string UserAgent { get; set; } = "Lumina-OpenLibrary/1.0";

    /// <summary>
    /// Gets or sets the contact email sent with every request to the Open Library API.
    /// </summary>
    public string? ContactEmail { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of results returned by a single search.
    /// </summary>
    public int SearchResultLimit { get; set; } = 10;

    /// <summary>
    /// Gets or sets the maximum number of editions fetched for a single work.
    /// </summary>
    public int WorkEditionLimit { get; set; } = 50;

    /// <summary>
    /// Gets or sets the minimum interval between consecutive requests to the Open Library API.
    /// </summary>
    public TimeSpan MinimumRequestInterval { get; set; } = TimeSpan.FromSeconds(1.1);
}
