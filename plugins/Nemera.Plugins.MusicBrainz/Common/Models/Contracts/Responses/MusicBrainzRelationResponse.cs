#region ========================================================================= USING =====================================================================================
using System.Collections.Generic;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Plugins.MusicBrainz.Common.Models.Contracts.Responses;

/// <summary>
/// Represents a response for a relationship between a MusicBrainz entity and another entity.
/// </summary>
internal sealed record MusicBrainzRelationResponse
{
    /// <summary>
    /// Gets the type of the relationship (e.g., producer, composer, member of band).
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; init; }

    /// <summary>
    /// Gets the MusicBrainz identifier of the type of the relationship.
    /// </summary>
    [JsonPropertyName("type-id")]
    public string? TypeId { get; init; }

    /// <summary>
    /// Gets the direction of the relationship.
    /// </summary>
    [JsonPropertyName("direction")]
    public string? Direction { get; init; }

    /// <summary>
    /// Gets the type of the entity the relationship targets.
    /// </summary>
    [JsonPropertyName("target-type")]
    public string? TargetType { get; init; }

    /// <summary>
    /// Gets the artist the relationship targets, when the target is an artist.
    /// </summary>
    [JsonPropertyName("artist")]
    public MusicBrainzArtistResponse? Artist { get; init; }

    /// <summary>
    /// Gets the work the relationship targets, when the target is a work.
    /// </summary>
    [JsonPropertyName("work")]
    public MusicBrainzWorkResponse? Work { get; init; }

    /// <summary>
    /// Gets the URL the relationship targets, when the target is a URL.
    /// </summary>
    [JsonPropertyName("url")]
    public MusicBrainzUrlResponse? Url { get; init; }

    /// <summary>
    /// Gets the attributes of the relationship (e.g., the instrument that was performed).
    /// </summary>
    [JsonPropertyName("attributes")]
    public List<string> Attributes { get; init; } = [];
}
