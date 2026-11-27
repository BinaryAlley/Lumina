#region ========================================================================= USING =====================================================================================
using System.Collections.Generic;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Plugins.CoverArtArchive.Common.Models.Contracts.Responses;

/// <summary>
/// Represents a response for a single image of the artwork of a release or a release group from the Cover Art Archive API.
/// </summary>
internal sealed record CoverArtArchiveImageResponse
{
    /// <summary>
    /// Gets the URL of the full size image.
    /// </summary>
    [JsonPropertyName("image")]
    public string? Image { get; init; }

    /// <summary>
    /// Gets the URL of the thumbnail of the image.
    /// </summary>
    [JsonPropertyName("thumbnail")]
    public string? Thumbnail { get; init; }

    /// <summary>
    /// Gets the types of the image, like "Front", "Back", "Booklet", "Medium", "Tray", "Spine" or "Poster".
    /// It is nullable because the deserializer writes a JSON null over the initializer, and may also contain null elements.
    /// </summary>
    [JsonPropertyName("types")]
    public List<string?>? Types { get; init; }

    /// <summary>
    /// Gets a value indicating whether the image is the front cover.
    /// </summary>
    [JsonPropertyName("front")]
    public bool IsFront { get; init; }

    /// <summary>
    /// Gets a value indicating whether the image is the back cover.
    /// </summary>
    [JsonPropertyName("back")]
    public bool IsBack { get; init; }

    /// <summary>
    /// Gets the optional comment describing the image.
    /// </summary>
    [JsonPropertyName("comment")]
    public string? Comment { get; init; }
}
