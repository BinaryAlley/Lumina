#region ========================================================================= USING =====================================================================================
using System.Collections.Generic;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Plugins.MusicBrainz.Common.Models.Contracts.Responses;

/// <summary>
/// Represents a response for an area from the MusicBrainz API.
/// </summary>
internal sealed record MusicBrainzAreaResponse
{
    /// <summary>
    /// Gets the MusicBrainz identifier of the area.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    /// <summary>
    /// Gets the name of the area.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>
    /// Gets the sort name of the area.
    /// </summary>
    [JsonPropertyName("sort-name")]
    public string? SortName { get; init; }

    /// <summary>
    /// Gets the disambiguation comment of the area.
    /// </summary>
    [JsonPropertyName("disambiguation")]
    public string? Disambiguation { get; init; }

    /// <summary>
    /// Gets the MusicBrainz type of the area.
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; init; }

    /// <summary>
    /// Gets the ISO 3166-1 codes of the area.
    /// </summary>
    [JsonPropertyName("iso-3166-1-codes")]
    public List<string> Iso3166Part1Codes { get; init; } = [];

    /// <summary>
    /// Gets the ISO 3166-2 codes of the area.
    /// </summary>
    [JsonPropertyName("iso-3166-2-codes")]
    public List<string> Iso3166Part2Codes { get; init; } = [];
}
