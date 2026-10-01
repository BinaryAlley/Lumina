#region ========================================================================= USING =====================================================================================
using System.Collections.Generic;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Plugins.MusicBrainz.Common.Models.Contracts.Responses;

/// <summary>
/// Represents a response for a work from the MusicBrainz API.
/// </summary>
internal sealed record MusicBrainzWorkResponse
{
    /// <summary>
    /// Gets the MusicBrainz identifier of the work.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    /// <summary>
    /// Gets the title of the work.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; init; }

    /// <summary>
    /// Gets the MusicBrainz type of the work.
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; init; }

    /// <summary>
    /// Gets the disambiguation comment of the work.
    /// </summary>
    [JsonPropertyName("disambiguation")]
    public string? Disambiguation { get; init; }

    /// <summary>
    /// Gets the languages of the work.
    /// </summary>
    [JsonPropertyName("languages")]
    public List<string> Languages { get; init; } = [];

    /// <summary>
    /// Gets the ISWC (International Standard Musical Work Code) of the work.
    /// </summary>
    [JsonPropertyName("iswcs")]
    public List<string> Iswcs { get; init; } = [];

    /// <summary>
    /// Gets the relationships of the work to other entities (e.g., its composers and lyricists).
    /// </summary>
    [JsonPropertyName("relations")]
    public List<MusicBrainzRelationResponse> Relations { get; init; } = [];

    /// <summary>
    /// Gets the tags of the work.
    /// </summary>
    [JsonPropertyName("tags")]
    public List<MusicBrainzTagResponse> Tags { get; init; } = [];

    /// <summary>
    /// Gets the genres of the work.
    /// </summary>
    [JsonPropertyName("genres")]
    public List<MusicBrainzTagResponse> Genres { get; init; } = [];
}
