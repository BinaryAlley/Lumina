#region ========================================================================= USING =====================================================================================
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Plugins.MusicBrainz.Common.Models.Contracts.Responses;

/// <summary>
/// Represents a response for the label information of a MusicBrainz release.
/// </summary>
internal sealed record MusicBrainzLabelInfoResponse
{
    /// <summary>
    /// Gets the catalog number assigned by the label.
    /// </summary>
    [JsonPropertyName("catalog-number")]
    public string? CatalogNumber { get; init; }

    /// <summary>
    /// Gets the label that issued the release.
    /// </summary>
    [JsonPropertyName("label")]
    public MusicBrainzLabelResponse? Label { get; init; }
}
