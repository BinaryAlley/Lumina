#region ========================================================================= USING =====================================================================================
using System.Collections.Generic;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Plugins.MusicBrainz.Common.Models.Contracts.Responses;

/// <summary>
/// Represents a response for an artist search from the MusicBrainz API.
/// </summary>
internal sealed record MusicBrainzArtistSearchResponse
{
    /// <summary>
    /// Gets the total number of matching artists.
    /// </summary>
    [JsonPropertyName("count")]
    public int Count { get; init; }

    /// <summary>
    /// Gets the offset of the returned artists.
    /// </summary>
    [JsonPropertyName("offset")]
    public int Offset { get; init; }

    /// <summary>
    /// Gets the matching artists.
    /// </summary>
    [JsonPropertyName("artists")]
    public List<MusicBrainzArtistResponse> Artists { get; init; } = [];
}
