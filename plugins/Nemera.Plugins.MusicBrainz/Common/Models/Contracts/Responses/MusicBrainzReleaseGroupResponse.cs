#region ========================================================================= USING =====================================================================================
using System.Collections.Generic;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Plugins.MusicBrainz.Common.Models.Contracts.Responses;

/// <summary>
/// Represents a response for a release group from the MusicBrainz API.
/// </summary>
internal sealed record MusicBrainzReleaseGroupResponse
{
    /// <summary>
    /// Gets the MusicBrainz identifier of the release group.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    /// <summary>
    /// Gets the title of the release group.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; init; }

    /// <summary>
    /// Gets the disambiguation comment of the release group.
    /// </summary>
    [JsonPropertyName("disambiguation")]
    public string? Disambiguation { get; init; }

    /// <summary>
    /// Gets the primary type of the release group.
    /// </summary>
    [JsonPropertyName("primary-type")]
    public string? PrimaryType { get; init; }

    /// <summary>
    /// Gets the secondary types of the release group.
    /// </summary>
    [JsonPropertyName("secondary-types")]
    public List<string> SecondaryTypes { get; init; } = [];

    /// <summary>
    /// Gets the date the release group was first released.
    /// </summary>
    [JsonPropertyName("first-release-date")]
    public string? FirstReleaseDate { get; init; }

    /// <summary>
    /// Gets the artist credit of the release group.
    /// </summary>
    [JsonPropertyName("artist-credit")]
    public List<MusicBrainzArtistCreditResponse> ArtistCredit { get; init; } = [];

    /// <summary>
    /// Gets the releases of the release group.
    /// </summary>
    [JsonPropertyName("releases")]
    public List<MusicBrainzReleaseResponse> Releases { get; init; } = [];

    /// <summary>
    /// Gets the tags of the release group.
    /// </summary>
    [JsonPropertyName("tags")]
    public List<MusicBrainzTagResponse> Tags { get; init; } = [];

    /// <summary>
    /// Gets the genres of the release group.
    /// </summary>
    [JsonPropertyName("genres")]
    public List<MusicBrainzTagResponse> Genres { get; init; } = [];

    /// <summary>
    /// Gets the aggregated rating of the release group.
    /// </summary>
    [JsonPropertyName("rating")]
    public MusicBrainzRatingResponse? Rating { get; init; }
}
