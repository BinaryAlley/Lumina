#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Plugins.OpenLibrary.Common.Models.Contracts.Responses;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Plugins.OpenLibrary.Fixtures.Common.Models.Contracts.Responses;

/// <summary>
/// Fixture class for the <see cref="OpenLibraryEditionResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
internal class OpenLibraryEditionResponseFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="OpenLibraryEditionResponse"/>.
    /// </summary>
    /// <param name="key">Optional. The key of the edition.</param>
    /// <param name="title">Optional. The title of the edition.</param>
    /// <param name="subtitle">Optional. The subtitle of the edition.</param>
    /// <param name="notes">Optional. The notes of the edition.</param>
    /// <param name="publishDate">Optional. The publication date of the edition.</param>
    /// <param name="publishers">Optional. The publishers of the edition.</param>
    /// <param name="publishPlaces">Optional. The publication places of the edition.</param>
    /// <param name="numberOfPages">Optional. The number of pages of the edition.</param>
    /// <param name="physicalFormat">Optional. The physical format of the edition.</param>
    /// <param name="editionName">Optional. The name of the edition.</param>
    /// <param name="series">Optional. The series of the edition.</param>
    /// <param name="volume">Optional. The volume of the edition within its series.</param>
    /// <param name="isbn10">Optional. The ISBN-10 identifiers of the edition.</param>
    /// <param name="isbn13">Optional. The ISBN-13 identifiers of the edition.</param>
    /// <param name="lccn">Optional. The LCCN identifiers of the edition.</param>
    /// <param name="oclcNumbers">Optional. The OCLC numbers of the edition.</param>
    /// <param name="identifiers">Optional. The external identifiers of the edition.</param>
    /// <param name="sourceRecords">Optional. The source records of the edition.</param>
    /// <param name="languages">Optional. The languages of the edition.</param>
    /// <param name="authors">Optional. The authors of the edition.</param>
    /// <param name="works">Optional. The works the edition belongs to.</param>
    /// <param name="contributions">Optional. The contributions of the edition.</param>
    /// <returns>The created edition response.</returns>
    public OpenLibraryEditionResponse Create(
        string? key = null,
        string? title = null,
        string? subtitle = null,
        JsonElement? notes = null,
        string? publishDate = null,
        List<string>? publishers = null,
        List<string>? publishPlaces = null,
        int? numberOfPages = null,
        string? physicalFormat = null,
        string? editionName = null,
        List<string>? series = null,
        string? volume = null,
        List<string>? isbn10 = null,
        List<string>? isbn13 = null,
        List<string>? lccn = null,
        List<string>? oclcNumbers = null,
        JsonElement? identifiers = null,
        List<string>? sourceRecords = null,
        List<OpenLibraryKeyReferenceResponse>? languages = null,
        List<OpenLibraryKeyReferenceResponse>? authors = null,
        List<OpenLibraryKeyReferenceResponse>? works = null,
        List<string>? contributions = null)
    {
        return new OpenLibraryEditionResponse
        {
            Key = key ?? $"/books/OL{_faker.Random.Number(1000, 9999)}M",
            Title = title ?? _faker.Commerce.ProductName(),
            Subtitle = subtitle,
            Notes = notes ?? default,
            PublishDate = publishDate,
            Publishers = publishers ?? [],
            PublishPlaces = publishPlaces ?? [],
            NumberOfPages = numberOfPages,
            PhysicalFormat = physicalFormat,
            EditionName = editionName,
            Series = series ?? [],
            Volume = volume,
            Isbn10 = isbn10 ?? [],
            Isbn13 = isbn13 ?? [],
            Lccn = lccn ?? [],
            OclcNumbers = oclcNumbers ?? [],
            Identifiers = identifiers ?? default,
            SourceRecords = sourceRecords ?? [],
            Languages = languages ?? [],
            Authors = authors ?? [],
            Works = works ?? [],
            Contributions = contributions ?? []
        };
    }
}
