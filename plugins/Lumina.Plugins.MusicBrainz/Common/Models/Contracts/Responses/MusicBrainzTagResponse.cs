#region ========================================================================= USING =====================================================================================
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Plugins.MusicBrainz.Common.Models.Contracts.Responses;

/// <summary>
/// Represents a response for a tag or a genre of a MusicBrainz entity.
/// </summary>
internal sealed record MusicBrainzTagResponse
{
    /// <summary>
    /// Gets the MusicBrainz identifier of the genre, when the tag is a genre.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    /// <summary>
    /// Gets the name of the tag.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>
    /// Gets the disambiguation comment of the genre, when the tag is a genre.
    /// </summary>
    [JsonPropertyName("disambiguation")]
    public string? Disambiguation { get; init; }

    /// <summary>
    /// Gets the number of votes the tag received.
    /// </summary>
    [JsonPropertyName("count")]
    public int Count { get; init; }
}
