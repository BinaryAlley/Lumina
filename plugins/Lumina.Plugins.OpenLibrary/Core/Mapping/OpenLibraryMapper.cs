#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using Lumina.Plugins.OpenLibrary.Common.Models.Contracts.Responses;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
#endregion

namespace Lumina.Plugins.OpenLibrary.Core.Mapping;

/// <summary>
/// Maps Open Library API responses into the application's book request DTOs.
/// </summary>
internal static partial class OpenLibraryMapper
{
    /// <summary>
    /// Matches a contribution in the form <c>Name (Role)</c>.
    /// </summary>
    [GeneratedRegex(@"^(?<name>.+?)\s*\((?<role>[^)]+)\)\s*$")]
    private static partial Regex ContributorPattern();

    /// <summary>
    /// Matches a volume number in a series or volume string.
    /// </summary>
    [GeneratedRegex(@"\b(?:volume|vol\.?|book|no\.?|#)\s*(?<number>\d+(?:\.\d+)?)\b", RegexOptions.IgnoreCase)]
    private static partial Regex VolumePattern();

    /// <summary>
    /// Matches a four digit year.
    /// </summary>
    [GeneratedRegex(@"\b(?:1[0-9]{3}|20[0-9]{2}|2100)\b")]
    private static partial Regex YearPattern();

    private static readonly HashSet<string> s_genreWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "adventure", "biography", "children's literature", "comics", "crime",
        "drama", "dystopian", "erotica", "essay", "fantasy", "fiction",
        "historical fiction", "history", "horror", "humor", "memoir", "mystery",
        "nonfiction", "poetry", "romance", "science fiction", "thriller",
        "travel", "young adult"
    };

    /// <summary>
    /// Maps the edition, work, author, rating, and search data into a full book request.
    /// </summary>
    /// <param name="lookup">The lookup describing the book to map.</param>
    /// <param name="edition">The edition data, or <see langword="null"/> when no edition was found.</param>
    /// <param name="work">The work data, or <see langword="null"/> when no work was found.</param>
    /// <param name="authors">The authors of the book.</param>
    /// <param name="ratings">The ratings of the book, or <see langword="null"/> when no ratings were found.</param>
    /// <param name="fallback">The search document used as a fallback source of data, or <see langword="null"/>.</param>
    /// <returns>The mapped book request.</returns>
    public static AddBookRequest MapDetailed(
        BookMetadataLookupDto lookup,
        OpenLibraryEditionResponse? edition,
        OpenLibraryWorkResponse? work,
        IReadOnlyList<OpenLibraryAuthorResponse> authors,
        OpenLibraryRatingsResponse? ratings,
        OpenLibrarySearchDocumentResponse? fallback = null)
    {
        List<string> subjects = DistinctValues(work?.Subjects ?? fallback?.Subjects ?? []);
        List<string> explicitGenres = DistinctValues(work?.Genres ?? []);
        List<string> genreNames = explicitGenres.Count > 0 ? explicitGenres : [.. subjects.Where(LooksLikeGenre)];

        (DateOnly? Date, int? Year) originalRelease = ParseOpenLibraryDate(work?.FirstPublishDate);
        int? originalYear = originalRelease.Date?.Year ?? originalRelease.Year ?? fallback?.FirstPublishYear;
        (DateOnly? Date, int? Year) editionRelease = ParseOpenLibraryDate(edition?.PublishDate);

        string? workId = ExtractOlid(work?.Key, 'W') ?? ExtractOlid(fallback?.Key, 'W');
        string? editionId = ExtractOlid(edition?.Key, 'M') ?? fallback?.EditionKeys.FirstOrDefault();

        string? currentLanguageCode = ExtractLanguageCode(edition?.Languages.FirstOrDefault()?.Key) ?? fallback?.Languages.FirstOrDefault();
        string? originalLanguageCode = ExtractLanguageCode(work?.OriginalLanguages.FirstOrDefault()?.Key);

        Dictionary<string, List<string>> identifiers = ReadIdentifiers(edition?.Identifiers ?? default);
        string? asin = FirstIdentifier(identifiers, "amazon", "asin") ?? FirstSourceIdentifier(edition?.SourceRecords, "amazon") ?? fallback?.AmazonIds.FirstOrDefault();
        string? goodreadsId = FirstIdentifier(identifiers, "goodreads") ?? fallback?.GoodreadsIds.FirstOrDefault();
        string? libraryThingId = FirstIdentifier(identifiers, "librarything") ?? fallback?.LibraryThingIds.FirstOrDefault();
        string? googleBooksId = FirstIdentifier(identifiers, "google", "google_books") ?? FirstSourceIdentifier(edition?.SourceRecords, "google") ?? fallback?.GoogleIds.FirstOrDefault();
        string? barnesAndNobleId = FirstIdentifier(identifiers, "barnesandnoble", "barnes_and_noble", "bn");
        string? appleBooksId = FirstIdentifier(identifiers, "apple", "apple_books", "ibooks");

        List<IsbnDto> isbns = MapIsbns(lookup.Isbn, edition, fallback);
        List<MediaContributorDto> contributors = MapContributors(authors, edition?.Contributions, fallback?.AuthorNames);
        List<BookRatingDto> mappedRatings = MapRatings(ratings, fallback);

        string? title = FirstNotEmpty(edition?.Title, work?.Title, fallback?.Title);
        string? description = ReadText(work?.Description ?? default) ?? ReadText(edition?.Notes ?? default);
        string? releaseCountry = edition?.PublishPlaces.FirstOrDefault() ?? fallback?.PublishPlaces.FirstOrDefault();
        string? releaseVersion = edition?.EditionName;

        return new AddBookRequest(
            lookup.LibraryId,
            lookup.Path,
            new WrittenContentMetadataDto(
                title,
                work?.OriginalTitle,
                description,
                new ReleaseInfoDto(
                    originalRelease.Date,
                    originalYear,
                    editionRelease.Date,
                    editionRelease.Date?.Year ?? editionRelease.Year,
                    NullIfWhiteSpace(releaseCountry),
                    NullIfWhiteSpace(releaseVersion)),
                [.. genreNames.Select(name => new GenreDto(name))],
                [.. subjects.Select(name => new TagDto(name))],
                MapLanguage(currentLanguageCode),
                MapLanguage(originalLanguageCode),
                edition?.Publishers.FirstOrDefault() ?? fallback?.Publishers.FirstOrDefault(),
                edition?.NumberOfPages ?? fallback?.NumberOfPagesMedian),
            MapFormat(edition?.PhysicalFormat),
            NullIfWhiteSpace(edition?.EditionName),
            ParseVolumeNumber(edition?.Volume, edition?.Series.FirstOrDefault()),
            MapSeries(edition?.Series.FirstOrDefault()),
            NullIfWhiteSpace(asin),
            NullIfWhiteSpace(goodreadsId),
            NullIfWhiteSpace(edition?.Lccn.FirstOrDefault() ?? fallback?.Lccn.FirstOrDefault()),
            NullIfWhiteSpace(edition?.OclcNumbers.FirstOrDefault() ?? fallback?.Oclc.FirstOrDefault()),
            editionId ?? workId,
            NullIfWhiteSpace(libraryThingId),
            NullIfWhiteSpace(googleBooksId),
            NullIfWhiteSpace(barnesAndNobleId),
            NullIfWhiteSpace(appleBooksId),
            isbns,
            contributors,
            mappedRatings);
    }

    /// <summary>
    /// Maps a search document into a book request candidate.
    /// </summary>
    /// <param name="lookup">The lookup describing the book to map.</param>
    /// <param name="document">The search document to map.</param>
    /// <returns>The mapped book request candidate.</returns>
    public static AddBookRequest MapSearchCandidate(BookMetadataLookupDto lookup, OpenLibrarySearchDocumentResponse document)
    {
        return MapDetailed(lookup, null, null, [], null, document);
    }

    /// <summary>
    /// Normalizes an ISBN into its canonical form without separators or whitespace.
    /// </summary>
    /// <param name="isbn">The ISBN to normalize.</param>
    /// <returns>The normalized ISBN.</returns>
    /// <exception cref="ArgumentException">The ISBN is empty, white space, or contains invalid characters.</exception>
    public static string NormalizeIsbn(string isbn)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(isbn);
        string normalized = new string([.. isbn.Where(character => character is not '-' && !char.IsWhiteSpace(character))]).ToUpperInvariant();

        if ((normalized.Length != 10 && normalized.Length != 13) ||
            normalized[..^1].Any(character => !char.IsDigit(character)) ||
            (!char.IsDigit(normalized[^1]) && !(normalized.Length == 10 && normalized[^1] == 'X')))
            throw new ArgumentException("The ISBN must contain 10 or 13 valid characters.", nameof(isbn));

        return normalized;
    }

    /// <summary>
    /// Extracts the canonical OLID from an Open Library key, when it matches the expected suffix.
    /// </summary>
    /// <param name="key">The Open Library key to extract the OLID from.</param>
    /// <param name="expectedSuffix">The suffix the OLID must end with, for example <c>W</c> for works.</param>
    /// <returns>The canonical OLID, or <see langword="null"/> when the key does not contain a matching OLID.</returns>
    public static string? ExtractOlid(string? key, char expectedSuffix)
    {
        if (string.IsNullOrWhiteSpace(key))
            return null;

        string candidate = key.Trim().TrimEnd('/').Split('/').Last().ToUpperInvariant();
        return candidate.StartsWith("OL", StringComparison.Ordinal) && candidate.EndsWith(expectedSuffix) && candidate.Length > 3 ? candidate : null;
    }

    /// <summary>
    /// Maps a physical format string into a book format enum value.
    /// </summary>
    /// <param name="physicalFormat">The physical format string to map.</param>
    /// <returns>The mapped book format, or <see langword="null"/> when the format is not recognized.</returns>
    public static BookFormat? MapFormat(string? physicalFormat)
    {
        if (string.IsNullOrWhiteSpace(physicalFormat))
            return null;

        string value = physicalFormat.Trim().ToLowerInvariant();
        if (value.Contains("mass market", StringComparison.Ordinal))
            return BookFormat.MassMarketPaperback;
        if (value.Contains("trade paperback", StringComparison.Ordinal))
            return BookFormat.TradePaperback;
        if (value.Contains("paperback", StringComparison.Ordinal) || value.Contains("softcover", StringComparison.Ordinal))
            return BookFormat.Paperback;
        if (value.Contains("hardcover", StringComparison.Ordinal) || value.Contains("hard cover", StringComparison.Ordinal) || value.Contains("hardback", StringComparison.Ordinal) || value.Contains("clothbound", StringComparison.Ordinal))
            return BookFormat.Hardcover;
        if (value.Contains("ebook", StringComparison.Ordinal) || value.Contains("e-book", StringComparison.Ordinal) || value.Contains("electronic", StringComparison.Ordinal))
            return BookFormat.eBook;
        if (value.Contains("audiobook", StringComparison.Ordinal) || value.Contains("audio book", StringComparison.Ordinal) || value.Contains("audio cd", StringComparison.Ordinal) || value.Contains("audio cassette", StringComparison.Ordinal))
            return BookFormat.Audiobook;
        if (value.Contains("large print", StringComparison.Ordinal))
            return BookFormat.LargePrint;
        if (value.Contains("board book", StringComparison.Ordinal))
            return BookFormat.BoardBook;
        if (value.Contains("spiral", StringComparison.Ordinal))
            return BookFormat.SpiralBound;
        if (value.Contains("library binding", StringComparison.Ordinal))
            return BookFormat.LibraryBinding;
        if (value.Contains("leather", StringComparison.Ordinal))
            return BookFormat.LeatherBound;
        if (value.Contains("pop-up", StringComparison.Ordinal) || value.Contains("popup", StringComparison.Ordinal))
            return BookFormat.PopupBook;
        return null;
    }

    /// <summary>
    /// Maps the ISBNs from the lookup, edition, and search document into ISBN DTOs, de-duplicating normalized values.
    /// </summary>
    /// <param name="lookupIsbn">The ISBN of the lookup.</param>
    /// <param name="edition">The edition data to read the ISBNs from.</param>
    /// <param name="fallback">The search document to read the ISBNs from.</param>
    /// <returns>The mapped and de-duplicated ISBN DTOs.</returns>
    private static List<IsbnDto> MapIsbns(string? lookupIsbn, OpenLibraryEditionResponse? edition, OpenLibrarySearchDocumentResponse? fallback)
    {
        List<IsbnDto> result = [];
        HashSet<string> seen = new(StringComparer.OrdinalIgnoreCase);

        void Add(string? raw, IsbnFormat? declaredFormat = null)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return;
            string normalized;
            try
            {
                normalized = NormalizeIsbn(raw);
            }
            catch (ArgumentException)
            {
                return;
            }

            if (!seen.Add(normalized))
                return;
            IsbnFormat format = declaredFormat ?? (normalized.Length == 10 ? IsbnFormat.Isbn10 : IsbnFormat.Isbn13);
            result.Add(new IsbnDto(normalized, format));
        }

        Add(lookupIsbn);
        foreach (string isbn in edition?.Isbn10 ?? [])
            Add(isbn, IsbnFormat.Isbn10);
        foreach (string isbn in edition?.Isbn13 ?? [])
            Add(isbn, IsbnFormat.Isbn13);
        foreach (string isbn in fallback?.Isbns ?? [])
            Add(isbn);
        return result;
    }

    /// <summary>
    /// Maps the authors and contributions into contributor DTOs, de-duplicating identical name and role pairs.
    /// </summary>
    /// <param name="authors">The authors of the book.</param>
    /// <param name="contributions">The contribution strings of the edition.</param>
    /// <param name="fallbackAuthors">The author names from the search document.</param>
    /// <returns>The mapped contributor DTOs.</returns>
    private static List<MediaContributorDto> MapContributors(IReadOnlyList<OpenLibraryAuthorResponse> authors, IReadOnlyList<string>? contributions, IReadOnlyList<string>? fallbackAuthors)
    {
        List<MediaContributorDto> result = [];
        HashSet<string> seen = new(StringComparer.OrdinalIgnoreCase);

        void Add(string? displayName, string? legalName, string role, MediaContributorRoleCategory category)
        {
            if (string.IsNullOrWhiteSpace(displayName) || !seen.Add($"{displayName}|{role}"))
                return;
            result.Add(new MediaContributorDto(new MediaContributorNameDto(displayName.Trim(), NullIfWhiteSpace(legalName)), new MediaContributorRoleDto(role, category)));
        }

        foreach (OpenLibraryAuthorResponse author in authors)
            Add(author.Name, author.PersonalName, "Author", MediaContributorRoleCategory.Author);
        foreach (string authorName in fallbackAuthors ?? [])
            Add(authorName, null, "Author", MediaContributorRoleCategory.Author);

        foreach (string contribution in contributions ?? [])
        {
            Match match = ContributorPattern().Match(contribution);
            string name = match.Success ? match.Groups["name"].Value.Trim() : contribution.Trim();
            string role = match.Success ? match.Groups["role"].Value.Trim() : "Contributor";
            Add(name, null, role, ContributorCategory(role));
        }
        return result;
    }

    /// <summary>
    /// Maps the ratings into a single Open Library rating DTO when an average or a count is present.
    /// </summary>
    /// <param name="ratings">The ratings of the work.</param>
    /// <param name="fallback">The search document to read the ratings from.</param>
    /// <returns>The mapped rating DTOs, or an empty list when no rating data was found.</returns>
    private static List<BookRatingDto> MapRatings(OpenLibraryRatingsResponse? ratings, OpenLibrarySearchDocumentResponse? fallback)
    {
        decimal? average = ratings?.Summary?.Average ?? fallback?.RatingsAverage;
        int? count = ratings?.Summary?.Count ?? fallback?.RatingsCount;
        if (average is null && count is null)
            return [];

        return [new BookRatingDto(average, 5m, BookRatingSource.OpenLibrary, count)];
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

        return culture is null ? new LanguageInfoDto(code, code, null) : new LanguageInfoDto(culture.TwoLetterISOLanguageName, culture.EnglishName, culture.NativeName);
    }

    /// <summary>
    /// Maps a series string into a book series DTO.
    /// </summary>
    /// <param name="series">The series string to map.</param>
    /// <returns>The mapped book series, or <see langword="null"/> when the string is empty.</returns>
    private static BookSeriesDto? MapSeries(string? series)
    {
        string? value = NullIfWhiteSpace(series);
        return value is null ? null : new BookSeriesDto(value);
    }

    /// <summary>
    /// Parses a volume number from the first candidate that contains one.
    /// </summary>
    /// <param name="candidates">The strings that may contain a volume number.</param>
    /// <returns>The parsed volume number, or <see langword="null"/> when no candidate contains one.</returns>
    private static float? ParseVolumeNumber(params string?[] candidates)
    {
        foreach (string? candidate in candidates)
        {
            if (string.IsNullOrWhiteSpace(candidate))
                continue;
            Match match = VolumePattern().Match(candidate);
            if (match.Success && float.TryParse(match.Groups["number"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out float number))
                return number;
        }
        return null;
    }

    /// <summary>
    /// Parses an Open Library date string into a date and a year.
    /// </summary>
    /// <param name="value">The date string to parse.</param>
    /// <returns>The parsed date and year, or <see langword="null"/> values when the string could not be parsed.</returns>
    private static (DateOnly? Date, int? Year) ParseOpenLibraryDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return (null, null);

        DateTimeStyles styles = DateTimeStyles.AllowWhiteSpaces;
        string[] formats =
        [
            "yyyy-MM-dd", "yyyy-M-d", "MMMM d, yyyy", "MMM d, yyyy",
            "d MMMM yyyy", "d MMM yyyy"
        ];

        if (DateOnly.TryParseExact(value.Trim(), formats, CultureInfo.InvariantCulture, styles, out DateOnly exact))
            return (exact, exact.Year);

        Match yearMatch = YearPattern().Match(value);
        return yearMatch.Success && int.TryParse(yearMatch.Value, out int year) ? (null, year) : (null, null);
    }

    /// <summary>
    /// Reads the text out of a JSON element that is either a plain string or an object with a <c>value</c> property.
    /// </summary>
    /// <param name="element">The JSON element to read.</param>
    /// <returns>The text, or <see langword="null"/> when the element does not contain readable text.</returns>
    private static string? ReadText(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => NullIfWhiteSpace(element.GetString()),
            JsonValueKind.Object when element.TryGetProperty("value", out JsonElement value) => ReadText(value),
            _ => null
        };
    }

    /// <summary>
    /// Reads an identifiers object into a dictionary keyed by identifier name.
    /// </summary>
    /// <param name="element">The JSON element to read.</param>
    /// <returns>The identifier values keyed by identifier name.</returns>
    private static Dictionary<string, List<string>> ReadIdentifiers(JsonElement element)
    {
        Dictionary<string, List<string>> result = new(StringComparer.OrdinalIgnoreCase);
        if (element.ValueKind != JsonValueKind.Object)
            return result;

        foreach (JsonProperty property in element.EnumerateObject())
        {
            IEnumerable<string?> values = property.Value.ValueKind switch
            {
                JsonValueKind.String => [property.Value.GetString()],
                JsonValueKind.Array => property.Value.EnumerateArray()
                    .Where(item => item.ValueKind == JsonValueKind.String)
                    .Select(item => item.GetString()),
                _ => []
            };

            result[property.Name] = [.. values
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value!)];
        }

        return result;
    }

    /// <summary>
    /// Gets the first value of the first identifier key that has values.
    /// </summary>
    /// <param name="identifiers">The identifiers to read from.</param>
    /// <param name="keys">The identifier keys to try, in order.</param>
    /// <returns>The first value, or <see langword="null"/> when none of the keys have values.</returns>
    private static string? FirstIdentifier(IReadOnlyDictionary<string, List<string>> identifiers, params string[] keys)
    {
        foreach (string key in keys)
            if (identifiers.TryGetValue(key, out List<string>? values) && values.Count > 0)
                return values[0];

        return null;
    }

    /// <summary>
    /// Gets the identifier value from a source record prefixed with the given source name.
    /// </summary>
    /// <param name="sourceRecords">The source records to read from.</param>
    /// <param name="source">The source name prefix to match.</param>
    /// <returns>The identifier value, or <see langword="null"/> when no source record matches.</returns>
    private static string? FirstSourceIdentifier(IReadOnlyList<string>? sourceRecords, string source)
    {
        string prefix = source + ":";
        string? match = sourceRecords?.FirstOrDefault(value => value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
        return match?[prefix.Length..];
    }

    /// <summary>
    /// Extracts the language code from an Open Library language key.
    /// </summary>
    /// <param name="key">The language key to extract the code from.</param>
    /// <returns>The language code, or <see langword="null"/> when the key is empty.</returns>
    private static string? ExtractLanguageCode(string? key)
    {
        return string.IsNullOrWhiteSpace(key) ? null : key.Trim().TrimEnd('/').Split('/').Last();
    }

    /// <summary>
    /// Trims, filters, and de-duplicates the given values, ignoring case.
    /// </summary>
    /// <param name="values">The values to process.</param>
    /// <returns>The processed values.</returns>
    private static List<string> DistinctValues(IEnumerable<string> values)
    {
        return [.. values.Select(value => value.Trim()).Where(value => value.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase)];
    }

    /// <summary>
    /// Determines whether a subject is likely to describe a genre.
    /// </summary>
    /// <param name="subject">The subject to check.</param>
    /// <returns><see langword="true"/> when the subject is a known genre word or ends in fiction or literature.</returns>
    private static bool LooksLikeGenre(string subject)
    {
        return s_genreWords.Contains(subject) ||
               subject.EndsWith(" fiction", StringComparison.OrdinalIgnoreCase) ||
               subject.EndsWith(" literature", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Maps a contribution role into the canonical category of the role.
    /// </summary>
    /// <param name="role">The role to categorize.</param>
    /// <returns>The canonical category of the role.</returns>
    private static MediaContributorRoleCategory ContributorCategory(string role)
    {
        if (role.Contains("illustr", StringComparison.OrdinalIgnoreCase) ||
            role.Contains("artist", StringComparison.OrdinalIgnoreCase))
            return MediaContributorRoleCategory.Illustrator;
        if (role.Contains("translat", StringComparison.OrdinalIgnoreCase))
            return MediaContributorRoleCategory.Translator;
        if (role.Contains("narrat", StringComparison.OrdinalIgnoreCase))
            return MediaContributorRoleCategory.Narrator;
        if (role.Contains("edit", StringComparison.OrdinalIgnoreCase) ||
            role.Contains("author", StringComparison.OrdinalIgnoreCase) ||
            role.Contains("writer", StringComparison.OrdinalIgnoreCase))
            return MediaContributorRoleCategory.Author;
        return MediaContributorRoleCategory.Other;
    }

    /// <summary>
    /// Gets the first value that is not empty or white space.
    /// </summary>
    /// <param name="values">The values to check, in order.</param>
    /// <returns>The first non-empty value, or <see langword="null"/> when all values are empty.</returns>
    private static string? FirstNotEmpty(params string?[] values)
    {
        return values.Select(NullIfWhiteSpace).FirstOrDefault(value => value is not null);
    }

    /// <summary>
    /// Returns the trimmed value, or <see langword="null"/> when the value is empty or white space.
    /// </summary>
    /// <param name="value">The value to process.</param>
    /// <returns>The trimmed value, or <see langword="null"/>.</returns>
    private static string? NullIfWhiteSpace(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
