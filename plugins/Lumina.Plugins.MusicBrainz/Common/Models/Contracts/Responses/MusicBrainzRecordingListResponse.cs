#region ========================================================================= USING =====================================================================================
using System.Collections.Generic;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Plugins.MusicBrainz.Common.Models.Contracts.Responses;

/// <summary>
/// Represents a response for the recordings returned by an ISRC lookup from the MusicBrainz API.
/// </summary>
internal sealed record MusicBrainzRecordingListResponse
{
    /// <summary>
    /// Gets the recordings that carry the looked up ISRC.
    /// </summary>
    [JsonPropertyName("recordings")]
    public List<MusicBrainzRecordingResponse> Recordings { get; init; } = [];
}
