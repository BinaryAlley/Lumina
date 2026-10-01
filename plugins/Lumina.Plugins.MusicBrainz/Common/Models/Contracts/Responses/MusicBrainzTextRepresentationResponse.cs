#region ========================================================================= USING =====================================================================================
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Plugins.MusicBrainz.Common.Models.Contracts.Responses;

/// <summary>
/// Represents a response for the language and script of the titles of a MusicBrainz release.
/// </summary>
internal sealed record MusicBrainzTextRepresentationResponse
{
    /// <summary>
    /// Gets the ISO 639-3 language code of the release.
    /// </summary>
    [JsonPropertyName("language")]
    public string? Language { get; init; }

    /// <summary>
    /// Gets the ISO 15924 script code of the release.
    /// </summary>
    [JsonPropertyName("script")]
    public string? Script { get; init; }
}
