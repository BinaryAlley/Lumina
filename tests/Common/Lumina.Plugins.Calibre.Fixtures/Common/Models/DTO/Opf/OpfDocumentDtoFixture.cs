#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.Calibre.Common.Models.DTO.Opf;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Plugins.Calibre.Fixtures.Common.Models.DTO.Opf;

/// <summary>
/// Fixture class for the <see cref="OpfDocumentDto"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class OpfDocumentDtoFixture
{
    /// <summary>
    /// Creates an <see cref="OpfDocumentDto"/>.
    /// </summary>
    /// <param name="title">Optional. The title of the book.</param>
    /// <param name="description">Optional. The description of the book, in HTML format.</param>
    /// <param name="publisher">Optional. The publisher of the book.</param>
    /// <param name="publishDate">Optional. The publication date of the book.</param>
    /// <param name="languageCode">Optional. The language code of the book.</param>
    /// <param name="series">Optional. The name of the series the book belongs to.</param>
    /// <param name="seriesIndex">Optional. The index of the book within its series.</param>
    /// <param name="rating">Optional. The personal rating of the book, on a scale of 0 to 10.</param>
    /// <param name="coverHref">Optional. The href of the cover image, relative to the directory of the book.</param>
    /// <param name="subjects">Optional. The subjects of the book, used as tags.</param>
    /// <param name="creators">Optional. The creators of the book.</param>
    /// <param name="contributors">Optional. The contributors of the book.</param>
    /// <param name="identifiers">Optional. The identifiers of the book, with their schemes.</param>
    /// <returns>The created <see cref="OpfDocumentDto"/>.</returns>
    public OpfDocumentDto Create(
        string? title = null,
        string? description = null,
        string? publisher = null,
        DateTimeOffset? publishDate = null,
        string? languageCode = null,
        string? series = null,
        double? seriesIndex = null,
        int? rating = null,
        string? coverHref = null,
        IEnumerable<string>? subjects = null,
        IEnumerable<OpfCreatorDto>? creators = null,
        IEnumerable<OpfContributorDto>? contributors = null,
        IEnumerable<OpfIdentifierDto>? identifiers = null)
    {
        OpfDocumentDto document = new()
        {
            Title = title,
            Description = description,
            Publisher = publisher,
            PublishDate = publishDate,
            LanguageCode = languageCode,
            Series = series,
            SeriesIndex = seriesIndex,
            Rating = rating,
            CoverHref = coverHref
        };
        if (subjects is not null)
            document.Subjects.AddRange(subjects);
        if (creators is not null)
            document.Creators.AddRange(creators);
        if (contributors is not null)
            document.Contributors.AddRange(contributors);
        if (identifiers is not null)
            document.Identifiers.AddRange(identifiers);
        return document;
    }

    /// <summary>
    /// Creates a list of <see cref="OpfDocumentDto"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<OpfDocumentDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
