#region ========================================================================= USING =====================================================================================
using System;
#endregion

namespace Lumina.Plugins.MusicBrainz.Common.Models.DTO.Settings;

/// <summary>
/// Data transfer object for the settings that configure the MusicBrainz metadata plugin.
/// </summary>
internal sealed class MusicBrainzSettingsDto
{
    /// <summary>
    /// Gets or sets the base URL of the MusicBrainz web service the requests are sent to.
    /// Defaults to the public MusicBrainz web service, and can point to a self hosted mirror instead.
    /// </summary>
    public string BaseUrl { get; set; } = "https://musicbrainz.org/ws/2/";

    /// <summary>
    /// Gets or sets a value indicating whether the base URL is allowed to point at a private or local host, for a self hosted mirror on a local
    /// network. Defaults to <see langword="false"/>, so that a misconfigured base URL cannot silently reach a local service.
    /// </summary>
    public bool DoesAllowPrivateBaseUrl { get; set; }

    /// <summary>
    /// Gets or sets the user agent sent with every request to the MusicBrainz API.
    /// </summary>
    public string UserAgent { get; set; } = "Lumina-MusicBrainz/1.0";

    /// <summary>
    /// Gets or sets the contact email sent with every request to the MusicBrainz API.
    /// </summary>
    public string? ContactEmail { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of results returned by a single search.
    /// </summary>
    public int SearchResultLimit { get; set; } = 10;

    /// <summary>
    /// Gets or sets the maximum number of releases fetched for a single release group.
    /// </summary>
    public int ReleaseLookupLimit { get; set; } = 25;

    /// <summary>
    /// Gets or sets the minimum interval between consecutive requests to the MusicBrainz API.
    /// MusicBrainz requires client applications to make no more than one request per second.
    /// </summary>
    public TimeSpan MinimumRequestInterval { get; set; } = TimeSpan.FromSeconds(1.0);
}
