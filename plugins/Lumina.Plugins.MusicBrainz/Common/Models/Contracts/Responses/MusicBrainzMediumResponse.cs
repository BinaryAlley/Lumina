#region ========================================================================= USING =====================================================================================
using System.Collections.Generic;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Plugins.MusicBrainz.Common.Models.Contracts.Responses;

/// <summary>
/// Represents a response for a medium of a MusicBrainz release.
/// </summary>
internal sealed record MusicBrainzMediumResponse
{
    /// <summary>
    /// Gets the position of the medium within its release.
    /// </summary>
    [JsonPropertyName("position")]
    public int Position { get; init; }

    /// <summary>
    /// Gets the title of the medium.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; init; }

    /// <summary>
    /// Gets the format of the medium (e.g., CD, Vinyl, Digital Media).
    /// </summary>
    [JsonPropertyName("format")]
    public string? Format { get; init; }

    /// <summary>
    /// Gets the number of tracks of the medium.
    /// </summary>
    [JsonPropertyName("track-count")]
    public int TrackCount { get; init; }

    /// <summary>
    /// Gets the tracks of the medium. The tracks are only returned when the recordings are included in the request.
    /// </summary>
    [JsonPropertyName("tracks")]
    public List<MusicBrainzTrackResponse> Tracks { get; init; } = [];
}
