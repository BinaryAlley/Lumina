#region ========================================================================= USING =====================================================================================
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Plugins.MusicBrainz.Common.Models.Contracts.Responses;

/// <summary>
/// Represents a response for the life span of a MusicBrainz artist, label, or area.
/// </summary>
internal sealed record MusicBrainzLifeSpanResponse
{
    /// <summary>
    /// Gets the begin date of the life span.
    /// </summary>
    [JsonPropertyName("begin")]
    public string? Begin { get; init; }

    /// <summary>
    /// Gets the end date of the life span.
    /// </summary>
    [JsonPropertyName("end")]
    public string? End { get; init; }

    /// <summary>
    /// Gets a value indicating whether the life span has ended.
    /// </summary>
    [JsonPropertyName("ended")]
    public bool? IsEnded { get; init; }
}
