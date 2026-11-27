#region ========================================================================= USING =====================================================================================
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Plugins.MusicBrainz.Common.Models.Contracts.Responses;

/// <summary>
/// Represents a response for the aggregated rating of a MusicBrainz entity.
/// </summary>
internal sealed record MusicBrainzRatingResponse
{
    /// <summary>
    /// Gets the average rating value.
    /// </summary>
    [JsonPropertyName("value")]
    public decimal? Value { get; init; }

    /// <summary>
    /// Gets the number of votes that contributed to the rating.
    /// </summary>
    [JsonPropertyName("votes-count")]
    public int? VotesCount { get; init; }
}
