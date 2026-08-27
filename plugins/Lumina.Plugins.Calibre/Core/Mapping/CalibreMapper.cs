#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using Lumina.Plugins.Calibre.Common.Models.DTO.Opf;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
#endregion

namespace Lumina.Plugins.Calibre.Core.Mapping;

/// <summary>
/// Maps the metadata read from a Calibre OPF file into the application's book metadata DTO.
/// </summary>
internal static class CalibreMapper
{
    /// <summary>
    /// Maps the metadata read from the OPF file into a book metadata DTO.
    /// </summary>
    /// <param name="document">The metadata read from the OPF file.</param>
    /// <returns>The mapped book metadata.</returns>
    public static BookMetadataDto Map(OpfDocumentDto document)
    {
        float? volumeNumber = document.SeriesIndex is > 0 ? (float?)document.SeriesIndex : null;

        return new BookMetadataDto(
            Title: document.Title,
            OriginalTitle: null,
            Description: StripHtml(document.Description),
            ReleaseInfo: BuildReleaseInfo(document.PublishDate),
            Genres: null,
            Tags: [.. document.Subjects.Select(subject => new TagDto(Name: subject))],
            Language: MapLanguage(document.LanguageCode),
            OriginalLanguage: null,
            Publisher: document.Publisher,
            PageCount: null,
            Format: null,
            Edition: null,
            VolumeNumber: volumeNumber,
            Series: document.Series is null ? null : new BookSeriesDto(Title: document.Series),
            ASIN: GetIdentifier(document.Identifiers, "asin", "amazon"),
            GoodreadsId: GetIdentifier(document.Identifiers, "goodreads"),
            LCCN: GetIdentifier(document.Identifiers, "lccn"),
            OCLCNumber: GetIdentifier(document.Identifiers, "oclc"),
            OpenLibraryId: GetIdentifier(document.Identifiers, "openlibrary"),
            LibraryThingId: null,
            GoogleBooksId: GetIdentifier(document.Identifiers, "google", "googlebooks"),
            BarnesAndNobleId: null,
            AppleBooksId: null,
            Isbns: BuildIsbns(document.Identifiers),
            Contributors: BuildContributors(document.Creators, document.Contributors),
            Ratings: document.Rating is null ? null : [new BookRatingDto(Value: document.Rating, MaxValue: 10m, Source: BookRatingSource.Calibre, VoteCount: null)],
            CoverImagePath: null
        );
    }

    /// <summary>
    /// Builds the release information of the book from its publication date.
    /// </summary>
    /// <param name="publishDate">The publication date of the book.</param>
    /// <returns>The release information of the book, always present even when the publication date is missing.</returns>
    private static ReleaseInfoDto BuildReleaseInfo(DateTimeOffset? publishDate)
    {
        // the release info must always be present, even when the publication date is missing, so that the metadata can be applied to the book
        if (publishDate is null)
        {
            return new ReleaseInfoDto(
                OriginalReleaseDate: null,
                OriginalReleaseYear: null,
                ReReleaseDate: null,
                ReReleaseYear: null,
                ReleaseCountry: null,
                ReleaseVersion: null
            );
        }

        DateOnly? releaseDate = DateOnly.FromDateTime(publishDate.Value.Date);
        return new ReleaseInfoDto(
            OriginalReleaseDate: releaseDate,
            OriginalReleaseYear: releaseDate?.Year,
            ReReleaseDate: null,
            ReReleaseYear: null,
            ReleaseCountry: null,
            ReleaseVersion: null
        );
    }

    /// <summary>
    /// Maps the ISBN identifiers of the book into ISBN DTOs, de-duplicating normalized values.
    /// </summary>
    /// <param name="identifiers">The identifiers read from the OPF file.</param>
    /// <returns>The mapped and de-duplicated ISBN DTOs.</returns>
    private static List<IsbnDto> BuildIsbns(List<OpfIdentifierDto> identifiers)
    {
        List<IsbnDto> result = [];
        HashSet<string> seen = new(StringComparer.OrdinalIgnoreCase);

        foreach (OpfIdentifierDto identifier in identifiers.Where(identifier => string.Equals(identifier.Scheme, "isbn", StringComparison.OrdinalIgnoreCase)))
        {
            string? normalized = NormalizeIsbn(identifier.Value);
            if (normalized is null || !seen.Add(normalized))
                continue;
            result.Add(new IsbnDto(Value: normalized, Format: normalized.Length == 10 ? IsbnFormat.Isbn10 : IsbnFormat.Isbn13));
        }

        return result;
    }

    /// <summary>
    /// Maps the creators and contributors of the book into contributor DTOs, de-duplicating identical name and role pairs.
    /// </summary>
    /// <param name="creators">The creators read from the OPF file.</param>
    /// <param name="contributors">The contributors read from the OPF file.</param>
    /// <returns>The mapped contributor DTOs.</returns>
    private static List<MediaContributorDto> BuildContributors(List<OpfCreatorDto> creators, List<OpfContributorDto> contributors)
    {
        List<MediaContributorDto> result = [];
        HashSet<string> seen = new(StringComparer.OrdinalIgnoreCase);

        void Add(string? displayName, string role, MediaContributorRoleCategory category)
        {
            if (string.IsNullOrWhiteSpace(displayName) || !seen.Add($"{displayName}|{role}"))
                return;
            result.Add(new MediaContributorDto(
                Name: new MediaContributorNameDto(DisplayName: displayName.Trim(), LegalName: null),
                Role: new MediaContributorRoleDto(Name: role, Category: category))
            );
        }

        foreach (OpfCreatorDto creator in creators)
            Add(creator.Name, "Author", MediaContributorRoleCategory.Author);
        foreach (OpfContributorDto contributor in contributors)
            Add(contributor.Name, MapContributorRole(contributor.Role), MapContributorCategory(contributor.Role));

        return result;
    }

    /// <summary>
    /// Maps a language code into a language info DTO by matching it against known cultures.
    /// </summary>
    /// <param name="rawCode">The raw language code to map.</param>
    /// <returns>The mapped language info, or <see langword="null"/> when the code is empty.</returns>
    private static LanguageInfoDto? MapLanguage(string? rawCode)
    {
        if (string.IsNullOrWhiteSpace(rawCode))
            return null;

        string code = rawCode.Trim().ToLowerInvariant();
        IEnumerable<CultureInfo> cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
            .Concat(CultureInfo.GetCultures(CultureTypes.NeutralCultures));

        CultureInfo? culture = cultures.FirstOrDefault(candidate =>
            string.Equals(candidate.TwoLetterISOLanguageName, code, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(candidate.ThreeLetterISOLanguageName, code, StringComparison.OrdinalIgnoreCase));

        return culture is null
            ? new LanguageInfoDto(LanguageCode: code, LanguageName: code, NativeName: null)
            : new LanguageInfoDto(LanguageCode: culture.TwoLetterISOLanguageName, LanguageName: culture.EnglishName, NativeName: culture.NativeName);
    }

    /// <summary>
    /// Strips the HTML markup of a description, leaving the text content.
    /// </summary>
    /// <param name="value">The HTML value to strip.</param>
    /// <returns>The text content, or <see langword="null"/> when the value is empty.</returns>
    private static string? StripHtml(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        string decoded = WebUtility.HtmlDecode(value);
        string withoutTags = Regex.Replace(decoded, "<[^>]+>", " ");
        string collapsed = Regex.Replace(withoutTags, @"\s+", " ").Trim();
        return collapsed.Length == 0 ? null : collapsed;
    }

    /// <summary>
    /// Normalizes an ISBN into its canonical form without separators or whitespace.
    /// </summary>
    /// <param name="isbn">The ISBN to normalize.</param>
    /// <returns>The normalized ISBN, or <see langword="null"/> when the value is not a valid ISBN.</returns>
    private static string? NormalizeIsbn(string isbn)
    {
        string normalized = new string([.. isbn.Where(character => character is not '-' && !char.IsWhiteSpace(character))]).ToUpperInvariant();
        // valid ISBNs must be 10 or 13 characters in length, all characters except the last one must be digits, and the last one must be a digit for ISBN-13 and X for ISBN-10
        if ((normalized.Length != 10 && normalized.Length != 13) ||
            normalized[..^1].Any(character => !char.IsDigit(character)) ||
            (!char.IsDigit(normalized[^1]) && !(normalized.Length == 10 && normalized[^1] == 'X')))
            return null;

        return normalized;
    }

    /// <summary>
    /// Gets the value of the first identifier whose scheme matches one of the <paramref name="schemes"/>.
    /// </summary>
    /// <param name="identifiers">The identifiers read from the OPF file.</param>
    /// <param name="schemes">The identifier schemes to try, in order.</param>
    /// <returns>The identifier value, or <see langword="null"/> when no identifier matches.</returns>
    private static string? GetIdentifier(List<OpfIdentifierDto> identifiers, params string[] schemes)
    {
        foreach (OpfIdentifierDto identifier in identifiers)
            if (schemes.Any(scheme => string.Equals(identifier.Scheme, scheme, StringComparison.OrdinalIgnoreCase)))
                return identifier.Value;

        return null;
    }

    /// <summary>
    /// Maps a creator or contributor role code into a role name.
    /// </summary>
    /// <param name="role">The role code to map.</param>
    /// <returns>The mapped role name.</returns>
    private static string MapContributorRole(string? role)
    {
        return string.Equals(role, "aut", StringComparison.OrdinalIgnoreCase) ? "Author" : string.IsNullOrWhiteSpace(role) ? "Contributor" : role;
    }

    /// <summary>
    /// Maps a creator or contributor role code into the canonical category of the role.
    /// </summary>
    /// <param name="role">The role code to map.</param>
    /// <returns>The canonical category of the role.</returns>
    private static MediaContributorRoleCategory MapContributorCategory(string? role)
    {
        if (string.Equals(role, "aut", StringComparison.OrdinalIgnoreCase) || role?.Contains("author", StringComparison.OrdinalIgnoreCase) == true)
            return MediaContributorRoleCategory.Author;
        if (string.Equals(role, "ill", StringComparison.OrdinalIgnoreCase))
            return MediaContributorRoleCategory.Illustrator;
        if (string.Equals(role, "trl", StringComparison.OrdinalIgnoreCase))
            return MediaContributorRoleCategory.Translator;
        if (string.Equals(role, "bkp", StringComparison.OrdinalIgnoreCase))
            return MediaContributorRoleCategory.Publisher;
        return MediaContributorRoleCategory.Other;
    }
}
