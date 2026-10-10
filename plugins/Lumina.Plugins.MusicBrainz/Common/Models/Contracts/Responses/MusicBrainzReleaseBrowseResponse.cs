#region ========================================================================= USING =====================================================================================
using System.Collections.Generic;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Plugins.MusicBrainz.Common.Models.Contracts.Responses;

/// <summary>
/// Represents a response for the result of browsing the releases linked to another MusicBrainz entity.
/// </summary>
internal sealed record MusicBrainzReleaseBrowseResponse
{
    /// <summary>
    /// Gets the releases returned by the browse request.
    /// </summary>
    [JsonPropertyName("releases")]
    public List<MusicBrainzReleaseResponse> Releases { get; init; } = [];

    /// <summary>
    /// Gets the total number of releases linked to the browsed entity.
    /// </summary>
    [JsonPropertyName("release-count")]
    public int ReleaseCount { get; init; }

    /// <summary>
    /// Gets the offset of the returned releases.
    /// </summary>
    [JsonPropertyName("release-offset")]
    public int ReleaseOffset { get; init; }
}
