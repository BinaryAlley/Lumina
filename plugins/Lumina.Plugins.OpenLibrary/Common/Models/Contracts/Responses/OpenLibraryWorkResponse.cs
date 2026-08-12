#region ========================================================================= USING =====================================================================================
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Plugins.OpenLibrary.Common.Models.Contracts.Responses;

/// <summary>
/// Represents a response for a work from the Open Library API.
/// </summary>
internal sealed record OpenLibraryWorkResponse
{
    /// <summary>
    /// Gets the key of the work.
    /// </summary>
    [JsonPropertyName("key")]
    public string? Key { get; init; }

    /// <summary>
    /// Gets the title of the work.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; init; }

    /// <summary>
    /// Gets the original title of the work.
    /// </summary>
    [JsonPropertyName("original_title")]
    public string? OriginalTitle { get; init; }

    /// <summary>
    /// Gets the description of the work.
    /// </summary>
    [JsonPropertyName("description")]
    public JsonElement Description { get; init; }

    /// <summary>
    /// Gets the first publication date of the work.
    /// </summary>
    [JsonPropertyName("first_publish_date")]
    public string? FirstPublishDate { get; init; }

    /// <summary>
    /// Gets the subjects of the work.
    /// </summary>
    [JsonPropertyName("subjects")]
    public List<string> Subjects { get; init; } = [];

    /// <summary>
    /// Gets the genres of the work.
    /// </summary>
    [JsonPropertyName("genres")]
    public List<string> Genres { get; init; } = [];

    /// <summary>
    /// Gets the authors of the work.
    /// </summary>
    [JsonPropertyName("authors")]
    public List<OpenLibraryWorkAuthorResponse> Authors { get; init; } = [];

    /// <summary>
    /// Gets the original languages of the work.
    /// </summary>
    [JsonPropertyName("original_languages")]
    public List<OpenLibraryKeyReferenceResponse> OriginalLanguages { get; init; } = [];
}
