#region ========================================================================= USING =====================================================================================
using System.Collections.Generic;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Plugins.MusicBrainz.Common.Models.Contracts.Responses;

/// <summary>
/// Represents a response for a recording from the MusicBrainz API.
/// </summary>
internal sealed record MusicBrainzRecordingResponse
{
    /// <summary>
    /// Gets the MusicBrainz identifier of the recording.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    /// <summary>
    /// Gets the title of the recording.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; init; }

    /// <summary>
    /// Gets the disambiguation comment of the recording.
    /// </summary>
    [JsonPropertyName("disambiguation")]
    public string? Disambiguation { get; init; }

    /// <summary>
    /// Gets the length of the recording in milliseconds.
    /// </summary>
    [JsonPropertyName("length")]
    public long? Length { get; init; }

    /// <summary>
    /// Gets a value indicating whether the recording is a video recording.
    /// </summary>
    [JsonPropertyName("video")]
    public bool? IsVideo { get; init; }

    /// <summary>
    /// Gets the date the recording was first released.
    /// </summary>
    [JsonPropertyName("first-release-date")]
    public string? FirstReleaseDate { get; init; }

    /// <summary>
    /// Gets the artist credit of the recording.
    /// </summary>
    [JsonPropertyName("artist-credit")]
    public List<MusicBrainzArtistCreditResponse> ArtistCredit { get; init; } = [];

    /// <summary>
    /// Gets the ISRC (International Standard Recording Code) of the recording.
    /// </summary>
    [JsonPropertyName("isrcs")]
    public List<string> Isrcs { get; init; } = [];

    /// <summary>
    /// Gets the releases the recording appears on.
    /// </summary>
    [JsonPropertyName("releases")]
    public List<MusicBrainzReleaseResponse> Releases { get; init; } = [];

    /// <summary>
    /// Gets the tags of the recording.
    /// </summary>
    [JsonPropertyName("tags")]
    public List<MusicBrainzTagResponse> Tags { get; init; } = [];

    /// <summary>
    /// Gets the genres of the recording.
    /// </summary>
    [JsonPropertyName("genres")]
    public List<MusicBrainzTagResponse> Genres { get; init; } = [];

    /// <summary>
    /// Gets the relationships of the recording to other entities (e.g., its work, and its performers).
    /// </summary>
    [JsonPropertyName("relations")]
    public List<MusicBrainzRelationResponse> Relations { get; init; } = [];

    /// <summary>
    /// Gets the aggregated rating of the recording.
    /// </summary>
    [JsonPropertyName("rating")]
    public MusicBrainzRatingResponse? Rating { get; init; }
}
