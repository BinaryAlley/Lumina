#region ========================================================================= USING =====================================================================================
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Plugins.MusicBrainz.Common.Models.Contracts.Responses;

/// <summary>
/// Represents a response for an alias of a MusicBrainz entity.
/// </summary>
internal sealed record MusicBrainzAliasResponse
{
    /// <summary>
    /// Gets the name of the alias.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>
    /// Gets the sort name of the alias.
    /// </summary>
    [JsonPropertyName("sort-name")]
    public string? SortName { get; init; }

    /// <summary>
    /// Gets the type of the alias.
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; init; }

    /// <summary>
    /// Gets the locale the alias is used in.
    /// </summary>
    [JsonPropertyName("locale")]
    public string? Locale { get; init; }

    /// <summary>
    /// Gets a value indicating whether this is the primary alias.
    /// </summary>
    [JsonPropertyName("primary")]
    public bool? IsPrimary { get; init; }

    /// <summary>
    /// Gets the begin date of the alias.
    /// </summary>
    [JsonPropertyName("begin")]
    public string? Begin { get; init; }

    /// <summary>
    /// Gets the end date of the alias.
    /// </summary>
    [JsonPropertyName("end")]
    public string? End { get; init; }

    /// <summary>
    /// Gets a value indicating whether the alias has ended.
    /// </summary>
    [JsonPropertyName("ended")]
    public bool? IsEnded { get; init; }
}
