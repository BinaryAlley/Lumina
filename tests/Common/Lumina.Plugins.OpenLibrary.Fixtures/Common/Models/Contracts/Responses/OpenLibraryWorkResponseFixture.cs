#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Plugins.OpenLibrary.Common.Models.Contracts.Responses;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Plugins.OpenLibrary.Fixtures.Common.Models.Contracts.Responses;

/// <summary>
/// Fixture class for the <see cref="OpenLibraryWorkResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
internal class OpenLibraryWorkResponseFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="OpenLibraryWorkResponse"/>.
    /// </summary>
    /// <param name="key">Optional. The key of the work.</param>
    /// <param name="title">Optional. The title of the work.</param>
    /// <param name="originalTitle">Optional. The original title of the work.</param>
    /// <param name="description">Optional. The description of the work.</param>
    /// <param name="firstPublishDate">Optional. The first publication date of the work.</param>
    /// <param name="subjects">Optional. The subjects of the work.</param>
    /// <param name="genres">Optional. The genres of the work.</param>
    /// <param name="authors">Optional. The authors of the work.</param>
    /// <param name="originalLanguages">Optional. The original languages of the work.</param>
    /// <returns>The created work response.</returns>
    public OpenLibraryWorkResponse Create(
        string? key = null,
        string? title = null,
        string? originalTitle = null,
        JsonElement? description = null,
        string? firstPublishDate = null,
        List<string>? subjects = null,
        List<string>? genres = null,
        List<OpenLibraryWorkAuthorResponse>? authors = null,
        List<OpenLibraryKeyReferenceResponse>? originalLanguages = null)
    {
        return new OpenLibraryWorkResponse
        {
            Key = key ?? $"/works/OL{_faker.Random.Number(1000, 9999)}W",
            Title = title ?? _faker.Commerce.ProductName(),
            OriginalTitle = originalTitle,
            Description = description ?? default,
            FirstPublishDate = firstPublishDate,
            Subjects = subjects ?? [],
            Genres = genres ?? [],
            Authors = authors ?? [],
            OriginalLanguages = originalLanguages ?? []
        };
    }
}
