#region ========================================================================= USING =====================================================================================
using System.Collections.Generic;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Plugins.MusicBrainz.Common.Models.Contracts.Responses;

/// <summary>
/// Represents a response for a release from the MusicBrainz API.
/// </summary>
internal sealed record MusicBrainzReleaseResponse
{
    /// <summary>
    /// Gets the MusicBrainz identifier of the release.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    /// <summary>
    /// Gets the title of the release.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; init; }

    /// <summary>
    /// Gets the disambiguation comment of the release.
    /// </summary>
    [JsonPropertyName("disambiguation")]
    public string? Disambiguation { get; init; }

    /// <summary>
    /// Gets the status of the release (e.g., Official, Bootleg).
    /// </summary>
    [JsonPropertyName("status")]
    public string? Status { get; init; }

    /// <summary>
    /// Gets the date the release was issued.
    /// </summary>
    [JsonPropertyName("date")]
    public string? Date { get; init; }

    /// <summary>
    /// Gets the ISO 3166-1 alpha-2 code of the country the release was issued in.
    /// </summary>
    [JsonPropertyName("country")]
    public string? Country { get; init; }

    /// <summary>
    /// Gets the release events of the release.
    /// </summary>
    [JsonPropertyName("release-events")]
    public List<MusicBrainzReleaseEventResponse> ReleaseEvents { get; init; } = [];

    /// <summary>
    /// Gets the barcode of the release.
    /// </summary>
    [JsonPropertyName("barcode")]
    public string? Barcode { get; init; }

    /// <summary>
    /// Gets the outermost packaging of the release.
    /// </summary>
    [JsonPropertyName("packaging")]
    public string? Packaging { get; init; }

    /// <summary>
    /// Gets the language and script of the titles of the release.
    /// </summary>
    [JsonPropertyName("text-representation")]
    public MusicBrainzTextRepresentationResponse? TextRepresentation { get; init; }

    /// <summary>
    /// Gets the artist credit of the release.
    /// </summary>
    [JsonPropertyName("artist-credit")]
    public List<MusicBrainzArtistCreditResponse> ArtistCredit { get; init; } = [];

    /// <summary>
    /// Gets the release group the release belongs to.
    /// </summary>
    [JsonPropertyName("release-group")]
    public MusicBrainzReleaseGroupResponse? ReleaseGroup { get; init; }

    /// <summary>
    /// Gets the label information of the release.
    /// </summary>
    [JsonPropertyName("label-info")]
    public List<MusicBrainzLabelInfoResponse> LabelInfo { get; init; } = [];

    /// <summary>
    /// Gets the media of the release.
    /// </summary>
    [JsonPropertyName("media")]
    public List<MusicBrainzMediumResponse> Media { get; init; } = [];

    /// <summary>
    /// Gets the ASIN (Amazon Standard Identification Number) of the release.
    /// </summary>
    [JsonPropertyName("asin")]
    public string? Asin { get; init; }

    /// <summary>
    /// Gets the tags of the release.
    /// </summary>
    [JsonPropertyName("tags")]
    public List<MusicBrainzTagResponse> Tags { get; init; } = [];

    /// <summary>
    /// Gets the genres of the release.
    /// </summary>
    [JsonPropertyName("genres")]
    public List<MusicBrainzTagResponse> Genres { get; init; } = [];

    /// <summary>
    /// Gets the relationships of the release to other entities (e.g., its producers and engineers).
    /// </summary>
    [JsonPropertyName("relations")]
    public List<MusicBrainzRelationResponse> Relations { get; init; } = [];
}
