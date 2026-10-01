#region ========================================================================= USING =====================================================================================
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Plugins.MusicBrainz.Common.Models.Contracts.Responses;

/// <summary>
/// Represents a response for a URL from the MusicBrainz API.
/// </summary>
internal sealed record MusicBrainzUrlResponse
{
    /// <summary>
    /// Gets the MusicBrainz identifier of the URL.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    /// <summary>
    /// Gets the resource the URL points to.
    /// </summary>
    [JsonPropertyName("resource")]
    public string? Resource { get; init; }
}
