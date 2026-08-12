#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Plugins.Contracts.Core.Metadata;
using Lumina.Plugins.OpenLibrary.Common.Models.Contracts.Responses;
using Lumina.Plugins.OpenLibrary.Common.Models.DTO.Settings;
using Lumina.Plugins.OpenLibrary.Core.Api;
using Lumina.Plugins.OpenLibrary.Core.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Plugins.OpenLibrary.Core;

/// <summary>
/// Provides book metadata from the Open Library by resolving lookups into book metadata DTOs.
/// </summary>
internal sealed class OpenLibraryBookMetadataProvider : IRemoteMetadataProvider<BookMetadataLookupDto, BookMetadataDto>
{
    private readonly OpenLibraryHttpClient _openLibraryHttpClient;
    private readonly OpenLibrarySettingsDto _openLibrarySettings;

    /// <summary>
    /// Gets the display name of the metadata provider.
    /// </summary>
    public string Name => "Open Library";

    /// <summary>
    /// Gets the media library type this metadata provider supports.
    /// </summary>
    public LibraryType SupportedLibraryType => LibraryType.Book;

    /// <summary>
    /// Gets a value indicating whether this metadata provider requires access to the web to retrieve metadata.
    /// </summary>
    public bool RequiresWebAccess => true;

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenLibraryBookMetadataProvider"/> class.
    /// </summary>
    /// <param name="openLibraryHttpClient">The HTTP client used to call the Open Library API.</param>
    /// <param name="openLibrarySettings">The settings that configure the Open Library API requests.</param>
    public OpenLibraryBookMetadataProvider(OpenLibraryHttpClient openLibraryHttpClient, OpenLibrarySettingsDto openLibrarySettings)
    {
        _openLibraryHttpClient = openLibraryHttpClient;
        _openLibrarySettings = openLibrarySettings;
    }

    /// <summary>
    /// Searches for the metadata of the media item described by <paramref name="bookMetadataLookup"/>.
    /// </summary>
    /// <param name="bookMetadataLookup">The lookup describing the media item to search for.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The collection of metadata candidates found for the media item.</returns>
    public async Task<IReadOnlyList<BookMetadataDto>> GetSearchResultsAsync(BookMetadataLookupDto bookMetadataLookup, CancellationToken cancellationToken)
    {
        ValidateLookup(bookMetadataLookup);

        if (!string.IsNullOrWhiteSpace(bookMetadataLookup.Isbn) || !string.IsNullOrWhiteSpace(bookMetadataLookup.OpenLibraryId))
        {
            AddBookRequest? exactBookRequestMatch = await GetBookAsync(bookMetadataLookup, cancellationToken).ConfigureAwait(false);
            return exactBookRequestMatch is null ? [] : [new BookMetadataDto(
                exactBookRequestMatch.Metadata?.Title,
                exactBookRequestMatch.Metadata?.OriginalTitle,
                exactBookRequestMatch.Metadata?.Description,
                exactBookRequestMatch.Metadata?.ReleaseInfo,
                exactBookRequestMatch.Metadata?.Genres,
                exactBookRequestMatch.Metadata?.Tags,
                exactBookRequestMatch.Metadata?.Language,
                exactBookRequestMatch.Metadata?.OriginalLanguage,
                exactBookRequestMatch.Metadata?.Publisher,
                exactBookRequestMatch.Metadata?.PageCount,
                exactBookRequestMatch.Format,
                exactBookRequestMatch.Edition,
                exactBookRequestMatch.VolumeNumber,
                exactBookRequestMatch.Series,
                exactBookRequestMatch.ASIN,
                exactBookRequestMatch.GoodreadsId,
                exactBookRequestMatch.LCCN,
                exactBookRequestMatch.OCLCNumber,
                exactBookRequestMatch.OpenLibraryId,
                exactBookRequestMatch.LibraryThingId,
                exactBookRequestMatch.GoogleBooksId,
                exactBookRequestMatch.BarnesAndNobleId,
                exactBookRequestMatch.AppleBooksId,
                exactBookRequestMatch.ISBNs,
                exactBookRequestMatch.Contributors,
                exactBookRequestMatch.Ratings,
                CoverImageUrl: null
            )];
        }

        IReadOnlyList<OpenLibrarySearchDocumentResponse> openLibrarySearchDocuments = await _openLibraryHttpClient.SearchAsync(bookMetadataLookup, _openLibrarySettings.SearchResultLimit, cancellationToken).ConfigureAwait(false);

        return [.. openLibrarySearchDocuments
            .Where(openLibrarySearchDocument => !string.IsNullOrWhiteSpace(openLibrarySearchDocument.Title))
            .Select(openLibrarySearchDocument =>
            {
                AddBookRequest addBookRequest = OpenLibraryMapper.MapSearchCandidate(bookMetadataLookup, openLibrarySearchDocument);
                return new BookMetadataDto(
                    addBookRequest.Metadata?.Title,
                    addBookRequest.Metadata?.OriginalTitle,
                    addBookRequest.Metadata?.Description,
                    addBookRequest.Metadata?.ReleaseInfo,
                    addBookRequest.Metadata?.Genres,
                    addBookRequest.Metadata?.Tags,
                    addBookRequest.Metadata?.Language,
                    addBookRequest.Metadata?.OriginalLanguage,
                    addBookRequest.Metadata?.Publisher,
                    addBookRequest.Metadata?.PageCount,
                    addBookRequest.Format,
                    addBookRequest.Edition,
                    addBookRequest.VolumeNumber,
                    addBookRequest.Series,
                    addBookRequest.ASIN,
                    addBookRequest.GoodreadsId,
                    addBookRequest.LCCN,
                    addBookRequest.OCLCNumber,
                    addBookRequest.OpenLibraryId,
                    addBookRequest.LibraryThingId,
                    addBookRequest.GoogleBooksId,
                    addBookRequest.BarnesAndNobleId,
                    addBookRequest.AppleBooksId,
                    addBookRequest.ISBNs,
                    addBookRequest.Contributors,
                    addBookRequest.Ratings,
                    CoverImageUrl: null
                );
            })];
    }

    /// <summary>
    /// Gets the metadata of the media item described by <paramref name="bookMetadataLookup"/>.
    /// </summary>
    /// <param name="bookMetadataLookup">The lookup describing the media item to get the metadata for.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The metadata of the media item, or <see langword="null"/> when no metadata was found.</returns>
    public async Task<BookMetadataDto?> GetMetadataAsync(BookMetadataLookupDto bookMetadataLookup, CancellationToken cancellationToken)
    {
        AddBookRequest? exactBookRequestMatch = await GetBookAsync(bookMetadataLookup, cancellationToken).ConfigureAwait(false);
        return exactBookRequestMatch is null ? null : new BookMetadataDto(
            exactBookRequestMatch.Metadata?.Title,
            exactBookRequestMatch.Metadata?.OriginalTitle,
            exactBookRequestMatch.Metadata?.Description,
            exactBookRequestMatch.Metadata?.ReleaseInfo,
            exactBookRequestMatch.Metadata?.Genres,
            exactBookRequestMatch.Metadata?.Tags,
            exactBookRequestMatch.Metadata?.Language,
            exactBookRequestMatch.Metadata?.OriginalLanguage,
            exactBookRequestMatch.Metadata?.Publisher,
            exactBookRequestMatch.Metadata?.PageCount,
            exactBookRequestMatch.Format,
            exactBookRequestMatch.Edition,
            exactBookRequestMatch.VolumeNumber,
            exactBookRequestMatch.Series,
            exactBookRequestMatch.ASIN,
            exactBookRequestMatch.GoodreadsId,
            exactBookRequestMatch.LCCN,
            exactBookRequestMatch.OCLCNumber,
            exactBookRequestMatch.OpenLibraryId,
            exactBookRequestMatch.LibraryThingId,
            exactBookRequestMatch.GoogleBooksId,
            exactBookRequestMatch.BarnesAndNobleId,
            exactBookRequestMatch.AppleBooksId,
            exactBookRequestMatch.ISBNs,
            exactBookRequestMatch.Contributors,
            exactBookRequestMatch.Ratings,
            CoverImageUrl: null
        );
    }

    /// <summary>
    /// Resolves the lookup into a full book request by combining edition, work, author, and rating data from Open Library.
    /// </summary>
    /// <param name="bookMetadataLookup">The lookup describing the book to resolve.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The resolved book request, or <see langword="null"/> when no matching book was found.</returns>
    public async Task<AddBookRequest?> GetBookAsync(BookMetadataLookupDto bookMetadataLookup, CancellationToken cancellationToken = default)
    {
        ValidateLookup(bookMetadataLookup);

        OpenLibraryEditionResponse? openLibraryEditionResponse = null;
        OpenLibraryWorkResponse? openLibraryWorkResponse = null;
        OpenLibrarySearchDocumentResponse? openLibrarySearchDocumentResponse = null;
        string? openLibraryId = bookMetadataLookup.OpenLibraryId;
        string? isbn = bookMetadataLookup.Isbn;
        // if we have an Open Library Id, that's the first thing we should search by
        if (!string.IsNullOrWhiteSpace(openLibraryId))
        {
            string olid = NormalizeOlid(openLibraryId);
            if (olid.EndsWith('M'))
                openLibraryEditionResponse = await _openLibraryHttpClient.GetEditionAsync(olid, cancellationToken).ConfigureAwait(false);
            else if (olid.EndsWith('W'))
            {
                // A work has no publisher-specific details on its own (no ISBN, page count, etc.), so we still need to pull it for its work-level fields (e.g. description, subjects).
                openLibraryWorkResponse = await _openLibraryHttpClient.GetWorkAsync(olid, cancellationToken).ConfigureAwait(false);
                // A work can have many editions; SelectEdition picks the one that best matches the lookup criteria (language, format, etc.) instead of assuming the first result is right.
                openLibraryEditionResponse = SelectEdition(await _openLibraryHttpClient.GetEditionsAsync(olid, _openLibrarySettings.WorkEditionLimit, cancellationToken).ConfigureAwait(false), bookMetadataLookup);
            }
            else
                throw new ArgumentException("A book Open Library ID must be a work ID ending in W or an edition ID ending in M.", nameof(bookMetadataLookup));

            if (openLibraryEditionResponse is null && openLibraryWorkResponse is null)
                return null;
        }
        // no Open Library Id, let's try by ISBN
        if (openLibraryEditionResponse is null && !string.IsNullOrWhiteSpace(isbn))
            openLibraryEditionResponse = await _openLibraryHttpClient.GetEditionByIsbnAsync(isbn, cancellationToken).ConfigureAwait(false);
        // if we still have no results, let's go nuclear and search by metadata
        if (openLibraryEditionResponse is null && openLibraryWorkResponse is null)
        {
            IReadOnlyList<OpenLibrarySearchDocumentResponse> searchResults = await _openLibraryHttpClient.SearchAsync(bookMetadataLookup, 1, cancellationToken).ConfigureAwait(false);
            if (searchResults.Count == 0 || searchResults[0] is null)
                return null;

            string? workId = OpenLibraryMapper.ExtractOlid(searchResults[0].Key, 'W');
            if (workId is not null)
            {
                openLibraryWorkResponse = await _openLibraryHttpClient.GetWorkAsync(workId, cancellationToken).ConfigureAwait(false);
                openLibraryEditionResponse = SelectEdition(await _openLibraryHttpClient.GetEditionsAsync(workId, _openLibrarySettings.WorkEditionLimit, cancellationToken).ConfigureAwait(false), bookMetadataLookup);
            }

            if (openLibraryEditionResponse is null && searchResults[0].EditionKeys.Count > 0)
                openLibraryEditionResponse = await _openLibraryHttpClient.GetEditionAsync(searchResults[0].EditionKeys[0], cancellationToken).ConfigureAwait(false);
        }
        // if by this time we still have no result, let's forget it...
        if (openLibraryEditionResponse is null && openLibraryWorkResponse is null && openLibrarySearchDocumentResponse is null)
            return null;

        string? editionWorkId = OpenLibraryMapper.ExtractOlid(openLibraryEditionResponse?.Works.FirstOrDefault()?.Key, 'W');
        if (openLibraryWorkResponse is null && editionWorkId is not null)
            openLibraryWorkResponse = await _openLibraryHttpClient.GetWorkAsync(editionWorkId, cancellationToken).ConfigureAwait(false);

        List<string> authorIds = [.. (openLibraryWorkResponse?.Authors
                .Select(item => OpenLibraryMapper.ExtractOlid(item.Author?.Key ?? item.Key, 'A')) ?? [])
            .Concat(openLibraryEditionResponse?.Authors
                .Select(item => OpenLibraryMapper.ExtractOlid(item.Key, 'A')) ?? [])
            .Where(id => id is not null)
            .Select(id => id!)
            .Distinct(StringComparer.OrdinalIgnoreCase)];

        List<OpenLibraryAuthorResponse> authors = [];
        foreach (string authorId in authorIds)
        {
            OpenLibraryAuthorResponse? author = await _openLibraryHttpClient.GetAuthorAsync(authorId, cancellationToken).ConfigureAwait(false);
            if (author is not null)
                authors.Add(author);
        }

        string? finalWorkId = OpenLibraryMapper.ExtractOlid(openLibraryWorkResponse?.Key, 'W') ?? editionWorkId;
        OpenLibraryRatingsResponse? ratings = finalWorkId is null ? null : await _openLibraryHttpClient.GetRatingsAsync(finalWorkId, cancellationToken).ConfigureAwait(false);

        return OpenLibraryMapper.MapDetailed(bookMetadataLookup, openLibraryEditionResponse, openLibraryWorkResponse, authors, ratings, openLibrarySearchDocumentResponse);
    }

    /// <summary>
    /// Selects the best matching edition from the given editions, favoring matches for the lookup's ISBN and language.
    /// </summary>
    /// <param name="openLibraryEditions">The editions to select from.</param>
    /// <param name="bookMetadataLookup">The lookup the selected edition must match.</param>
    /// <returns>The best matching edition, or <see langword="null"/> when no edition was found.</returns>
    private static OpenLibraryEditionResponse? SelectEdition(IReadOnlyList<OpenLibraryEditionResponse> openLibraryEditions, BookMetadataLookupDto bookMetadataLookup)
    {
        string? lookupIsbn = bookMetadataLookup.Isbn;
        string? wantedIsbn = string.IsNullOrWhiteSpace(lookupIsbn) ? null : OpenLibraryMapper.NormalizeIsbn(lookupIsbn);
        string? wantedLanguage = bookMetadataLookup.LanguageCode?.Trim();

        return openLibraryEditions
            .Select((edition, index) => new
            {
                Edition = edition,
                Index = index,
                Score = ScoreEdition(edition, wantedIsbn, wantedLanguage)
            })
            .OrderByDescending(item => item.Score)
            .ThenBy(item => item.Index)
            .Select(item => item.Edition)
            .FirstOrDefault();
    }

    /// <summary>
    /// Scores an edition against the wanted ISBN and language to rank how well it matches the lookup.
    /// </summary>
    /// <param name="openLibraryEdition">The edition to score.</param>
    /// <param name="wantedIsbn">The normalized ISBN to match, or <see langword="null"/> when the lookup has no ISBN.</param>
    /// <param name="wantedLanguage">The language code to match, or <see langword="null"/> when the lookup has no language.</param>
    /// <returns>The match score of the edition, where a higher score is a better match.</returns>
    private static int ScoreEdition(OpenLibraryEditionResponse openLibraryEdition, string? wantedIsbn, string? wantedLanguage)
    {
        int score = 0;
        // if we get an ISBN match, that's the most certain identification score
        if (wantedIsbn is not null && openLibraryEdition.Isbn10.Concat(openLibraryEdition.Isbn13).Any(value => string.Equals(SafeNormalizeIsbn(value), wantedIsbn, StringComparison.OrdinalIgnoreCase)))
            score += 10_000;

        if (!string.IsNullOrWhiteSpace(wantedLanguage) && openLibraryEdition.Languages.Any(language =>
                string.Equals(language.Key?.Trim().TrimEnd('/').Split('/').Last(), wantedLanguage, StringComparison.OrdinalIgnoreCase)))
            score += 1_000;

        if (openLibraryEdition.NumberOfPages is > 0)
            score += 100;
        if (openLibraryEdition.Isbn10.Count + openLibraryEdition.Isbn13.Count > 0)
            score += 50;
        if (openLibraryEdition.Publishers.Count > 0)
            score += 20;
        if (!string.IsNullOrWhiteSpace(openLibraryEdition.PublishDate))
            score += 10;
        if (!string.IsNullOrWhiteSpace(openLibraryEdition.EditionName))
            score += 5;
        return score;
    }

    /// <summary>
    /// Normalizes an ISBN, returning <see langword="null"/> when the value is not a valid ISBN.
    /// </summary>
    /// <param name="value">The ISBN to normalize.</param>
    /// <returns>The normalized ISBN, or <see langword="null"/> when the value is not a valid ISBN.</returns>
    private static string? SafeNormalizeIsbn(string value)
    {
        try
        {
            return OpenLibraryMapper.NormalizeIsbn(value);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    /// <summary>
    /// Normalizes a raw Open Library identifier into its canonical OLID form.
    /// </summary>
    /// <param name="raw">The raw Open Library identifier to normalize.</param>
    /// <returns>The canonical OLID.</returns>
    /// <exception cref="ArgumentException">The value is not a valid Open Library identifier.</exception>
    private static string NormalizeOlid(string raw)
    {
        string id = raw.Trim().TrimEnd('/').Split('/').Last().ToUpperInvariant();
        if (id.Length < 4 || !id.StartsWith("OL", StringComparison.Ordinal) || (id[^1] is not ('M' or 'W')) || !id.AsSpan(2, id.Length - 3).ToString().All(char.IsDigit))
            throw new ArgumentException("The Open Library ID is invalid.", nameof(raw));

        return id;
    }

    /// <summary>
    /// Validates that the lookup contains the information required to resolve book metadata.
    /// </summary>
    /// <param name="bookMetadataLookup">The lookup to validate.</param>
    /// <exception cref="ArgumentException">The lookup is missing required information.</exception>
    private static void ValidateLookup(BookMetadataLookupDto bookMetadataLookup)
    {
        ArgumentNullException.ThrowIfNull(bookMetadataLookup);
        if (bookMetadataLookup.LibraryId == Guid.Empty)
            throw new ArgumentException("LibraryId is required.", nameof(bookMetadataLookup));

        ArgumentException.ThrowIfNullOrWhiteSpace(bookMetadataLookup.Path);

        if (string.IsNullOrWhiteSpace(bookMetadataLookup.Isbn) && string.IsNullOrWhiteSpace(bookMetadataLookup.OpenLibraryId) && string.IsNullOrWhiteSpace(bookMetadataLookup.Title))
            throw new ArgumentException("At least one of Isbn, OpenLibraryId, or Title is required.", nameof(bookMetadataLookup));
    }
}
