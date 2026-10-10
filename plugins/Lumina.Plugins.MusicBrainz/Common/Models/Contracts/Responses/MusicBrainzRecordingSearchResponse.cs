#region ========================================================================= USING =====================================================================================
using System.Collections.Generic;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Plugins.MusicBrainz.Common.Models.Contracts.Responses;

/// <summary>
/// Represents a response for a recording search from the MusicBrainz API.
/// </summary>
internal sealed record MusicBrainzRecordingSearchResponse
{
    /// <summary>
    /// Gets the total number of matching recordings.
    /// </summary>
    [JsonPropertyName("count")]
    public int Count { get; init; }

    /// <summary>
    /// Gets the offset of the returned recordings.
    /// </summary>
    [JsonPropertyName("offset")]
    public int Offset { get; init; }

    /// <summary>
    /// Gets the matching recordings.
    /// </summary>
    [JsonPropertyName("recordings")]
    public List<MusicBrainzRecordingResponse> Recordings { get; init; } = [];
}
