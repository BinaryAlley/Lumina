#region ========================================================================= USING =====================================================================================
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Plugins.OpenLibrary.Common.Models.Contracts.Responses;

/// <summary>
/// Represents a response for an edition from the Open Library API.
/// </summary>
internal sealed record OpenLibraryEditionResponse
{
    /// <summary>
    /// Gets the key of the edition.
    /// </summary>
    [JsonPropertyName("key")]
    public string? Key { get; init; }

    /// <summary>
    /// Gets the title of the edition.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; init; }

    /// <summary>
    /// Gets the subtitle of the edition.
    /// </summary>
    [JsonPropertyName("subtitle")]
    public string? Subtitle { get; init; }

    /// <summary>
    /// Gets the notes of the edition.
    /// </summary>
    [JsonPropertyName("notes")]
    public JsonElement Notes { get; init; }

    /// <summary>
    /// Gets the publication date of the edition.
    /// </summary>
    [JsonPropertyName("publish_date")]
    public string? PublishDate { get; init; }

    /// <summary>
    /// Gets the publishers of the edition.
    /// </summary>
    [JsonPropertyName("publishers")]
    public List<string> Publishers { get; init; } = [];

    /// <summary>
    /// Gets the publication places of the edition.
    /// </summary>
    [JsonPropertyName("publish_places")]
    public List<string> PublishPlaces { get; init; } = [];

    /// <summary>
    /// Gets the number of pages of the edition.
    /// </summary>
    [JsonPropertyName("number_of_pages")]
    public int? NumberOfPages { get; init; }

    /// <summary>
    /// Gets the physical format of the edition.
    /// </summary>
    [JsonPropertyName("physical_format")]
    public string? PhysicalFormat { get; init; }

    /// <summary>
    /// Gets the name of the edition.
    /// </summary>
    [JsonPropertyName("edition_name")]
    public string? EditionName { get; init; }

    /// <summary>
    /// Gets the series the edition belongs to.
    /// </summary>
    [JsonPropertyName("series")]
    public List<string> Series { get; init; } = [];

    /// <summary>
    /// Gets the volume of the edition within its series.
    /// </summary>
    [JsonPropertyName("volume")]
    public string? Volume { get; init; }

    /// <summary>
    /// Gets the ISBN-10 identifiers of the edition.
    /// </summary>
    [JsonPropertyName("isbn_10")]
    public List<string> Isbn10 { get; init; } = [];

    /// <summary>
    /// Gets the ISBN-13 identifiers of the edition.
    /// </summary>
    [JsonPropertyName("isbn_13")]
    public List<string> Isbn13 { get; init; } = [];

    /// <summary>
    /// Gets the LCCN identifiers of the edition.
    /// </summary>
    [JsonPropertyName("lccn")]
    public List<string> Lccn { get; init; } = [];

    /// <summary>
    /// Gets the OCLC numbers of the edition.
    /// </summary>
    [JsonPropertyName("oclc_numbers")]
    public List<string> OclcNumbers { get; init; } = [];

    /// <summary>
    /// Gets the external identifiers of the edition, keyed by identifier name.
    /// </summary>
    [JsonPropertyName("identifiers")]
    public JsonElement Identifiers { get; init; }

    /// <summary>
    /// Gets the source records of the edition.
    /// </summary>
    [JsonPropertyName("source_records")]
    public List<string> SourceRecords { get; init; } = [];

    /// <summary>
    /// Gets the languages of the edition.
    /// </summary>
    [JsonPropertyName("languages")]
    public List<OpenLibraryKeyReferenceResponse> Languages { get; init; } = [];

    /// <summary>
    /// Gets the authors of the edition.
    /// </summary>
    [JsonPropertyName("authors")]
    public List<OpenLibraryKeyReferenceResponse> Authors { get; init; } = [];

    /// <summary>
    /// Gets the works the edition belongs to.
    /// </summary>
    [JsonPropertyName("works")]
    public List<OpenLibraryKeyReferenceResponse> Works { get; init; } = [];

    /// <summary>
    /// Gets the contributions of the edition.
    /// </summary>
    [JsonPropertyName("contributions")]
    public List<string> Contributions { get; init; } = [];
}
