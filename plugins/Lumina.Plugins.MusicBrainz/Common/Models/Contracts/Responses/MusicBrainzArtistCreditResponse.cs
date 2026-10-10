#region ========================================================================= USING =====================================================================================
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Plugins.MusicBrainz.Common.Models.Contracts.Responses;

/// <summary>
/// Represents a response for a single credit in the artist credit of a MusicBrainz entity.
/// </summary>
internal sealed record MusicBrainzArtistCreditResponse
{
    /// <summary>
    /// Gets the credited name of the artist.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>
    /// Gets the join phrase that follows the credit.
    /// </summary>
    [JsonPropertyName("joinphrase")]
    public string? JoinPhrase { get; init; }

    /// <summary>
    /// Gets the artist the credit refers to.
    /// </summary>
    [JsonPropertyName("artist")]
    public MusicBrainzArtistResponse? Artist { get; init; }
}
