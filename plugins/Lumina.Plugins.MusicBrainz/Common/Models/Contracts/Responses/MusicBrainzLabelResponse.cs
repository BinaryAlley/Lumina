#region ========================================================================= USING =====================================================================================
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Plugins.MusicBrainz.Common.Models.Contracts.Responses;

/// <summary>
/// Represents a response for a label from the MusicBrainz API.
/// </summary>
internal sealed record MusicBrainzLabelResponse
{
    /// <summary>
    /// Gets the MusicBrainz identifier of the label.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    /// <summary>
    /// Gets the name of the label.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>
    /// Gets the disambiguation comment of the label.
    /// </summary>
    [JsonPropertyName("disambiguation")]
    public string? Disambiguation { get; init; }

    /// <summary>
    /// Gets the label code of the label.
    /// </summary>
    [JsonPropertyName("label-code")]
    public int? LabelCode { get; init; }
}
