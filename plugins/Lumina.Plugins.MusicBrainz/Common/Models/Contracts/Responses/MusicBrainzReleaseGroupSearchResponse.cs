#region ========================================================================= USING =====================================================================================
using System.Collections.Generic;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Plugins.MusicBrainz.Common.Models.Contracts.Responses;

/// <summary>
/// Represents a response for a release group search from the MusicBrainz API.
/// </summary>
internal sealed record MusicBrainzReleaseGroupSearchResponse
{
    /// <summary>
    /// Gets the total number of matching release groups.
    /// </summary>
    [JsonPropertyName("count")]
    public int Count { get; init; }

    /// <summary>
    /// Gets the offset of the returned release groups.
    /// </summary>
    [JsonPropertyName("offset")]
    public int Offset { get; init; }

    /// <summary>
    /// Gets the matching release groups.
    /// </summary>
    [JsonPropertyName("release-groups")]
    public List<MusicBrainzReleaseGroupResponse> ReleaseGroups { get; init; } = [];
}
