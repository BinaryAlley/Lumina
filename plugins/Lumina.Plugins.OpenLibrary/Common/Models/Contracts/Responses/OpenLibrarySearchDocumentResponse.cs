#region ========================================================================= USING =====================================================================================
using System.Collections.Generic;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Plugins.OpenLibrary.Common.Models.Contracts.Responses;

/// <summary>
/// Represents a response for a search document from the Open Library API.
/// </summary>
internal sealed record OpenLibrarySearchDocumentResponse
{
    /// <summary>
    /// Gets the key of the search document.
    /// </summary>
    [JsonPropertyName("key")]
    public string? Key { get; init; }

    /// <summary>
    /// Gets the title of the search document.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; init; }

    /// <summary>
    /// Gets the author names of the search document.
    /// </summary>
    [JsonPropertyName("author_name")]
    public List<string> AuthorNames { get; init; } = [];

    /// <summary>
    /// Gets the author keys of the search document.
    /// </summary>
    [JsonPropertyName("author_key")]
    public List<string> AuthorKeys { get; init; } = [];

    /// <summary>
    /// Gets the first publication year of the search document.
    /// </summary>
    [JsonPropertyName("first_publish_year")]
    public int? FirstPublishYear { get; init; }

    /// <summary>
    /// Gets the edition keys of the search document.
    /// </summary>
    [JsonPropertyName("edition_key")]
    public List<string> EditionKeys { get; init; } = [];

    /// <summary>
    /// Gets the ISBNs of the search document.
    /// </summary>
    [JsonPropertyName("isbn")]
    public List<string> Isbns { get; init; } = [];

    /// <summary>
    /// Gets the languages of the search document.
    /// </summary>
    [JsonPropertyName("language")]
    public List<string> Languages { get; init; } = [];

    /// <summary>
    /// Gets the publishers of the search document.
    /// </summary>
    [JsonPropertyName("publisher")]
    public List<string> Publishers { get; init; } = [];

    /// <summary>
    /// Gets the subjects of the search document.
    /// </summary>
    [JsonPropertyName("subject")]
    public List<string> Subjects { get; init; } = [];

    /// <summary>
    /// Gets the publication places of the search document.
    /// </summary>
    [JsonPropertyName("publish_place")]
    public List<string> PublishPlaces { get; init; } = [];

    /// <summary>
    /// Gets the median number of pages of the search document.
    /// </summary>
    [JsonPropertyName("number_of_pages_median")]
    public int? NumberOfPagesMedian { get; init; }

    /// <summary>
    /// Gets the average rating of the search document.
    /// </summary>
    [JsonPropertyName("ratings_average")]
    public decimal? RatingsAverage { get; init; }

    /// <summary>
    /// Gets the number of ratings of the search document.
    /// </summary>
    [JsonPropertyName("ratings_count")]
    public int? RatingsCount { get; init; }

    /// <summary>
    /// Gets the Amazon identifiers of the search document.
    /// </summary>
    [JsonPropertyName("id_amazon")]
    public List<string> AmazonIds { get; init; } = [];

    /// <summary>
    /// Gets the Goodreads identifiers of the search document.
    /// </summary>
    [JsonPropertyName("id_goodreads")]
    public List<string> GoodreadsIds { get; init; } = [];

    /// <summary>
    /// Gets the Google identifiers of the search document.
    /// </summary>
    [JsonPropertyName("id_google")]
    public List<string> GoogleIds { get; init; } = [];

    /// <summary>
    /// Gets the LibraryThing identifiers of the search document.
    /// </summary>
    [JsonPropertyName("id_librarything")]
    public List<string> LibraryThingIds { get; init; } = [];

    /// <summary>
    /// Gets the LCCN identifiers of the search document.
    /// </summary>
    [JsonPropertyName("lccn")]
    public List<string> Lccn { get; init; } = [];

    /// <summary>
    /// Gets the OCLC identifiers of the search document.
    /// </summary>
    [JsonPropertyName("oclc")]
    public List<string> Oclc { get; init; } = [];
}
