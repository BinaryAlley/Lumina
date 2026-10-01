#region ========================================================================= USING =====================================================================================
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Plugins.MusicBrainz.Common.Models.Contracts.Responses;

/// <summary>
/// Represents a response for a release event of a MusicBrainz release.
/// </summary>
internal sealed record MusicBrainzReleaseEventResponse
{
    /// <summary>
    /// Gets the date of the release event.
    /// </summary>
    [JsonPropertyName("date")]
    public string? Date { get; init; }

    /// <summary>
    /// Gets the area of the release event.
    /// </summary>
    [JsonPropertyName("area")]
    public MusicBrainzAreaResponse? Area { get; init; }
}
