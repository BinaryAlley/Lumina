#region ========================================================================= USING =====================================================================================
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Plugins.MusicBrainz.Common.Models.Contracts.Responses;

/// <summary>
/// Represents a response for a track of a medium of a MusicBrainz release, including the recording it is a performance of.
/// </summary>
internal sealed record MusicBrainzTrackResponse
{
    /// <summary>
    /// Gets the MusicBrainz identifier of the track.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    /// <summary>
    /// Gets the track number, as displayed on the medium (for example, "1" or "A1").
    /// </summary>
    [JsonPropertyName("number")]
    public string? Number { get; init; }

    /// <summary>
    /// Gets the position of the track within its medium.
    /// </summary>
    [JsonPropertyName("position")]
    public int Position { get; init; }

    /// <summary>
    /// Gets the title of the track.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; init; }

    /// <summary>
    /// Gets the length of the track in milliseconds.
    /// </summary>
    [JsonPropertyName("length")]
    public long? Length { get; init; }

    /// <summary>
    /// Gets the recording the track is a performance of.
    /// </summary>
    [JsonPropertyName("recording")]
    public MusicBrainzRecordingResponse? Recording { get; init; }
}
