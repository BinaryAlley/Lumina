#region ========================================================================= USING =====================================================================================
using System;
#endregion

namespace Lumina.Plugins.CoverArtArchive.Common.Models.DTO.Settings;

/// <summary>
/// Data transfer object for the settings that configure the Cover Art Archive artwork plugin.
/// </summary>
internal sealed class CoverArtArchiveSettingsDto
{
    /// <summary>
    /// Gets or sets the user agent sent with every request to the Cover Art Archive API.
    /// </summary>
    public string UserAgent { get; set; } = "Lumina-CoverArtArchive/1.0";

    /// <summary>
    /// Gets or sets the contact email sent with every request to the Cover Art Archive API.
    /// The MetaBrainz services expect a meaningful user agent that carries a way to contact its author.
    /// </summary>
    public string? ContactEmail { get; set; }

    /// <summary>
    /// Gets or sets the minimum interval between consecutive requests to the Cover Art Archive API, so that the public service is not overwhelmed.
    /// </summary>
    public TimeSpan MinimumRequestInterval { get; set; } = TimeSpan.FromSeconds(1.0);
}
