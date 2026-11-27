#region ========================================================================= USING =====================================================================================
using System.Collections.Generic;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Plugins.MusicBrainz.Common.Models.Contracts.Responses;

/// <summary>
/// Represents a response for an artist from the MusicBrainz API.
/// </summary>
internal sealed record MusicBrainzArtistResponse
{
    /// <summary>
    /// Gets the MusicBrainz identifier of the artist.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    /// <summary>
    /// Gets the name of the artist.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>
    /// Gets the sort name of the artist.
    /// </summary>
    [JsonPropertyName("sort-name")]
    public string? SortName { get; init; }

    /// <summary>
    /// Gets the disambiguation comment of the artist.
    /// </summary>
    [JsonPropertyName("disambiguation")]
    public string? Disambiguation { get; init; }

    /// <summary>
    /// Gets the MusicBrainz type of the artist.
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; init; }

    /// <summary>
    /// Gets the gender of the artist.
    /// </summary>
    [JsonPropertyName("gender")]
    public string? Gender { get; init; }

    /// <summary>
    /// Gets the ISO 3166-1 alpha-2 code of the country of the artist.
    /// </summary>
    [JsonPropertyName("country")]
    public string? Country { get; init; }

    /// <summary>
    /// Gets the area the artist is primarily identified with.
    /// </summary>
    [JsonPropertyName("area")]
    public MusicBrainzAreaResponse? Area { get; init; }

    /// <summary>
    /// Gets the area the artist began in.
    /// </summary>
    [JsonPropertyName("begin-area")]
    public MusicBrainzAreaResponse? BeginArea { get; init; }

    /// <summary>
    /// Gets the area the artist ended in.
    /// </summary>
    [JsonPropertyName("end-area")]
    public MusicBrainzAreaResponse? EndArea { get; init; }

    /// <summary>
    /// Gets the life span of the artist.
    /// </summary>
    [JsonPropertyName("life-span")]
    public MusicBrainzLifeSpanResponse? LifeSpan { get; init; }

    /// <summary>
    /// Gets the ISNI codes of the artist.
    /// </summary>
    [JsonPropertyName("isnis")]
    public List<string> Isnis { get; init; } = [];

    /// <summary>
    /// Gets the IPI codes of the artist.
    /// </summary>
    [JsonPropertyName("ipis")]
    public List<string> Ipis { get; init; } = [];

    /// <summary>
    /// Gets the alternative names of the artist.
    /// </summary>
    [JsonPropertyName("aliases")]
    public List<MusicBrainzAliasResponse> Aliases { get; init; } = [];

    /// <summary>
    /// Gets the tags of the artist.
    /// </summary>
    [JsonPropertyName("tags")]
    public List<MusicBrainzTagResponse> Tags { get; init; } = [];

    /// <summary>
    /// Gets the genres of the artist.
    /// </summary>
    [JsonPropertyName("genres")]
    public List<MusicBrainzTagResponse> Genres { get; init; } = [];

    /// <summary>
    /// Gets the relationships of the artist to other entities.
    /// </summary>
    [JsonPropertyName("relations")]
    public List<MusicBrainzRelationResponse> Relations { get; init; } = [];

    /// <summary>
    /// Gets the aggregated rating of the artist.
    /// </summary>
    [JsonPropertyName("rating")]
    public MusicBrainzRatingResponse? Rating { get; init; }
}
