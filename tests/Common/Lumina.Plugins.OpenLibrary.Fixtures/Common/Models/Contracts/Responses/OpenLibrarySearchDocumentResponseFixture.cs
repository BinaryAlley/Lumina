#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Plugins.OpenLibrary.Common.Models.Contracts.Responses;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Plugins.OpenLibrary.Fixtures.Common.Models.Contracts.Responses;

/// <summary>
/// Fixture class for the <see cref="OpenLibrarySearchDocumentResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
internal class OpenLibrarySearchDocumentResponseFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="OpenLibrarySearchDocumentResponse"/>.
    /// </summary>
    /// <param name="key">Optional. The key of the search document.</param>
    /// <param name="title">Optional. The title of the search document.</param>
    /// <param name="authorNames">Optional. The author names of the search document.</param>
    /// <param name="authorKeys">Optional. The author keys of the search document.</param>
    /// <param name="firstPublishYear">Optional. The first publication year of the search document.</param>
    /// <param name="editionKeys">Optional. The edition keys of the search document.</param>
    /// <param name="isbns">Optional. The ISBNs of the search document.</param>
    /// <param name="languages">Optional. The languages of the search document.</param>
    /// <param name="publishers">Optional. The publishers of the search document.</param>
    /// <param name="subjects">Optional. The subjects of the search document.</param>
    /// <param name="publishPlaces">Optional. The publication places of the search document.</param>
    /// <param name="numberOfPagesMedian">Optional. The median number of pages of the search document.</param>
    /// <param name="ratingsAverage">Optional. The average rating of the search document.</param>
    /// <param name="ratingsCount">Optional. The number of ratings of the search document.</param>
    /// <param name="amazonIds">Optional. The Amazon identifiers of the search document.</param>
    /// <param name="goodreadsIds">Optional. The Goodreads identifiers of the search document.</param>
    /// <param name="googleIds">Optional. The Google identifiers of the search document.</param>
    /// <param name="libraryThingIds">Optional. The LibraryThing identifiers of the search document.</param>
    /// <param name="lccn">Optional. The LCCN identifiers of the search document.</param>
    /// <param name="oclc">Optional. The OCLC identifiers of the search document.</param>
    /// <returns>The created search document response.</returns>
    public OpenLibrarySearchDocumentResponse Create(
        string? key = null,
        string? title = null,
        List<string>? authorNames = null,
        List<string>? authorKeys = null,
        int? firstPublishYear = null,
        List<string>? editionKeys = null,
        List<string>? isbns = null,
        List<string>? languages = null,
        List<string>? publishers = null,
        List<string>? subjects = null,
        List<string>? publishPlaces = null,
        int? numberOfPagesMedian = null,
        decimal? ratingsAverage = null,
        int? ratingsCount = null,
        List<string>? amazonIds = null,
        List<string>? goodreadsIds = null,
        List<string>? googleIds = null,
        List<string>? libraryThingIds = null,
        List<string>? lccn = null,
        List<string>? oclc = null)
    {
        return new OpenLibrarySearchDocumentResponse
        {
            Key = key ?? $"/works/OL{_faker.Random.Number(1000, 9999)}W",
            Title = title ?? _faker.Commerce.ProductName(),
            AuthorNames = authorNames ?? [],
            AuthorKeys = authorKeys ?? [],
            FirstPublishYear = firstPublishYear,
            EditionKeys = editionKeys ?? [],
            Isbns = isbns ?? [],
            Languages = languages ?? [],
            Publishers = publishers ?? [],
            Subjects = subjects ?? [],
            PublishPlaces = publishPlaces ?? [],
            NumberOfPagesMedian = numberOfPagesMedian,
            RatingsAverage = ratingsAverage,
            RatingsCount = ratingsCount,
            AmazonIds = amazonIds ?? [],
            GoodreadsIds = goodreadsIds ?? [],
            GoogleIds = googleIds ?? [],
            LibraryThingIds = libraryThingIds ?? [],
            Lccn = lccn ?? [],
            Oclc = oclc ?? []
        };
    }
}
