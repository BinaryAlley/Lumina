#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Plugins.OpenLibrary.Common.Models.Contracts.Responses;
using Lumina.Plugins.OpenLibrary.Common.Models.DTO.Settings;
using Lumina.Plugins.OpenLibrary.Core.Mapping;
using Lumina.Plugins.OpenLibrary.Core.Settings;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Plugins.OpenLibrary.Core.Api;

/// <summary>
/// HTTP client that calls the Open Library API and deserializes its JSON responses into typed response models.
/// </summary>
internal sealed class OpenLibraryHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly OpenLibrarySettingsProvider _settingsProvider;
    private readonly SemaphoreSlim _requestGate = new(1, 1);
    private DateTimeOffset _lastRequestAt = DateTimeOffset.MinValue;

    private static readonly JsonSerializerOptions s_serializerOptions = new(JsonSerializerDefaults.Web)
    {
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    private const string SEARCH_FIELDS =
        "key,title,author_name,author_key,first_publish_year,edition_key,isbn," +
        "language,publisher,subject,publish_place,number_of_pages_median," +
        "ratings_average,ratings_count,id_amazon,id_goodreads,id_google," +
        "id_librarything,lccn,oclc";

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenLibraryHttpClient"/> class.
    /// </summary>
    /// <param name="httpClient">The <see cref="HttpClient"/> used to send requests to the Open Library API.</param>
    /// <param name="settingsProvider">The provider of the settings that configure the Open Library API requests.</param>
    public OpenLibraryHttpClient(HttpClient httpClient, OpenLibrarySettingsProvider settingsProvider)
    {
        _httpClient = httpClient;
        _settingsProvider = settingsProvider;

        _httpClient.BaseAddress ??= new("https://openlibrary.org/");
    }

    /// <summary>
    /// Applies the request headers configured in the <paramref name="settings"/> onto the underlying HTTP client, unless they are already set.
    /// </summary>
    /// <param name="settings">The settings that configure the Open Library API requests.</param>
    private void EnsureRequestHeaders(OpenLibrarySettingsDto settings)
    {
        if (_httpClient.DefaultRequestHeaders.UserAgent.Count == 0)
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(settings.UserAgent);

        if (!string.IsNullOrWhiteSpace(settings.ContactEmail) && _httpClient.DefaultRequestHeaders.From is null)
            _httpClient.DefaultRequestHeaders.From = settings.ContactEmail!;
    }

    /// <summary>
    /// Gets an edition by its ISBN.
    /// </summary>
    /// <param name="isbn">The ISBN of the edition to get.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The edition, or <see langword="null"/> when no edition was found for the ISBN.</returns>
    public Task<OpenLibraryEditionResponse?> GetEditionByIsbnAsync(string isbn, CancellationToken cancellationToken)
    {
        string normalized = OpenLibraryMapper.NormalizeIsbn(isbn);
        return GetJsonOrNullAsync<OpenLibraryEditionResponse>($"isbn/{Uri.EscapeDataString(normalized)}.json", cancellationToken);
    }

    /// <summary>
    /// Gets an edition by its Open Library edition identifier.
    /// </summary>
    /// <param name="editionId">The Open Library edition identifier of the edition to get.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The edition, or <see langword="null"/> when no edition was found for the identifier.</returns>
    public Task<OpenLibraryEditionResponse?> GetEditionAsync(string editionId, CancellationToken cancellationToken)
    {
        return GetJsonOrNullAsync<OpenLibraryEditionResponse>($"books/{Uri.EscapeDataString(NormalizeOlid(editionId, 'M'))}.json", cancellationToken);
    }

    /// <summary>
    /// Gets a work by its Open Library work identifier.
    /// </summary>
    /// <param name="workId">The Open Library work identifier of the work to get.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The work, or <see langword="null"/> when no work was found for the identifier.</returns>
    public Task<OpenLibraryWorkResponse?> GetWorkAsync(string workId, CancellationToken cancellationToken)
    {
        return GetJsonOrNullAsync<OpenLibraryWorkResponse>($"works/{Uri.EscapeDataString(NormalizeOlid(workId, 'W'))}.json", cancellationToken);
    }

    /// <summary>
    /// Gets an author by its Open Library author identifier.
    /// </summary>
    /// <param name="authorId">The Open Library author identifier of the author to get.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The author, or <see langword="null"/> when no author was found for the identifier.</returns>
    public Task<OpenLibraryAuthorResponse?> GetAuthorAsync(string authorId, CancellationToken cancellationToken)
    {
        return GetJsonOrNullAsync<OpenLibraryAuthorResponse>($"authors/{Uri.EscapeDataString(NormalizeOlid(authorId, 'A'))}.json", cancellationToken);
    }

    /// <summary>
    /// Gets the ratings of a work by its Open Library work identifier.
    /// </summary>
    /// <param name="workId">The Open Library work identifier of the work to get the ratings for.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The ratings, or <see langword="null"/> when no ratings were found for the work.</returns>
    public Task<OpenLibraryRatingsResponse?> GetRatingsAsync(string workId, CancellationToken cancellationToken)
    {
        return GetJsonOrNullAsync<OpenLibraryRatingsResponse>($"works/{Uri.EscapeDataString(NormalizeOlid(workId, 'W'))}/ratings.json", cancellationToken);
    }

    /// <summary>
    /// Gets a list of editions for a work by its Open Library work identifier.
    /// </summary>
    /// <param name="workId">The Open Library work identifier of the work to get the editions for.</param>
    /// <param name="limit">The maximum number of editions to get.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The list of editions for the work.</returns>
    public async Task<IReadOnlyList<OpenLibraryEditionResponse>> GetEditionsAsync(string workId, int limit, CancellationToken cancellationToken)
    {
        OpenLibraryEditionsResponse? response = await GetJsonOrNullAsync<OpenLibraryEditionsResponse>(
                $"works/{Uri.EscapeDataString(NormalizeOlid(workId, 'W'))}/editions.json" +
                $"?limit={Math.Clamp(limit, 1, 1000).ToString(CultureInfo.InvariantCulture)}",
                cancellationToken)
            .ConfigureAwait(false);

        return response?.Entries ?? [];
    }

    /// <summary>
    /// Searches Open Library for books matching the lookup.
    /// </summary>
    /// <param name="lookup">The lookup describing the book to search for.</param>
    /// <param name="limit">The maximum number of search results to return.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The list of search results for the lookup.</returns>
    public async Task<IReadOnlyList<OpenLibrarySearchDocumentResponse>> SearchAsync(BookMetadataLookupDto lookup, int limit, CancellationToken cancellationToken)
    {
        List<KeyValuePair<string, string>> parameters = [];

        string? isbn = lookup.Isbn;
        string? title = lookup.Title;
        string? author = lookup.Author;
        string? languageCode = lookup.LanguageCode;

        if (!string.IsNullOrWhiteSpace(isbn))
            parameters.Add(new("isbn", OpenLibraryMapper.NormalizeIsbn(isbn)));

        if (!string.IsNullOrWhiteSpace(title))
            parameters.Add(new("title", title.Trim()));

        if (!string.IsNullOrWhiteSpace(author))
            parameters.Add(new("author", author.Trim()));

        if (!string.IsNullOrWhiteSpace(languageCode))
            parameters.Add(new("language", languageCode.Trim()));

        parameters.Add(new("fields", SEARCH_FIELDS));
        parameters.Add(new("limit", Math.Clamp(limit, 1, 100).ToString(CultureInfo.InvariantCulture)));

        string query = string.Join("&", parameters.Select(pair => $"{Uri.EscapeDataString(pair.Key)}={Uri.EscapeDataString(pair.Value)}"));

        OpenLibrarySearchResponse? response = await GetJsonOrNullAsync<OpenLibrarySearchResponse>($"search.json?{query}", cancellationToken).ConfigureAwait(false);

        return response?.Documents ?? [];
    }

    /// <summary>
    /// Gets the JSON resource at the given relative URL and deserializes it into <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the JSON response into.</typeparam>
    /// <param name="relativeUrl">The URL of the resource, relative to the base address.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The deserialized resource, or <see langword="null"/> when the resource was not found.</returns>
    /// <remarks>Transient failures and rate limiting are handled by retrying the request up to three times.</remarks>
    private async Task<T?> GetJsonOrNullAsync<T>(string relativeUrl, CancellationToken cancellationToken)
    {
        for (int attempt = 0; attempt < 3; attempt++)
        {
            using HttpResponseMessage response = await SendAsync(relativeUrl, cancellationToken).ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return default;

            if (response.IsSuccessStatusCode)
            {
                await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
                return await JsonSerializer.DeserializeAsync<T>(stream, s_serializerOptions, cancellationToken).ConfigureAwait(false);
            }

            if (attempt < 2 && (response.StatusCode == HttpStatusCode.TooManyRequests || (int)response.StatusCode >= 500))
            {
                TimeSpan retryDelay = response.Headers.RetryAfter?.Delta ?? TimeSpan.FromMilliseconds(250 * Math.Pow(2, attempt));
                await Task.Delay(retryDelay, cancellationToken).ConfigureAwait(false);
                continue;
            }

            response.EnsureSuccessStatusCode();
        }

        return default;
    }

    /// <summary>
    /// Sends a GET request to the Open Library API, throttled to respect the configured minimum request interval.
    /// </summary>
    /// <param name="relativeUrl">The URL of the resource, relative to the base address.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The HTTP response message for the request.</returns>
    private async Task<HttpResponseMessage> SendAsync(string relativeUrl, CancellationToken cancellationToken)
    {
        OpenLibrarySettingsDto settings = await _settingsProvider.GetAsync(cancellationToken).ConfigureAwait(false);
        EnsureRequestHeaders(settings);

        await _requestGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            DateTimeOffset earliestNextRequest = _lastRequestAt + settings.MinimumRequestInterval;
            TimeSpan delay = earliestNextRequest - DateTimeOffset.UtcNow;
            if (delay > TimeSpan.Zero)
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);

            HttpRequestMessage request = new(HttpMethod.Get, relativeUrl);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            _lastRequestAt = DateTimeOffset.UtcNow;
            return response;
        }
        finally
        {
            _requestGate.Release();
        }
    }

    /// <summary>
    /// Normalizes a raw Open Library identifier into its canonical OLID form.
    /// </summary>
    /// <param name="value">The raw Open Library identifier to normalize.</param>
    /// <param name="expectedSuffix">The suffix the OLID must end with, for example <c>M</c> for editions.</param>
    /// <returns>The canonical OLID.</returns>
    /// <exception cref="ArgumentException">The value is not a valid Open Library identifier.</exception>
    private static string NormalizeOlid(string value, char expectedSuffix)
    {
        string id = value.Trim().TrimEnd('/').Split('/').Last().ToUpperInvariant();
        if (id.Length < 4 || !id.StartsWith("OL", StringComparison.Ordinal) || id[^1] != expectedSuffix || !id.AsSpan(2, id.Length - 3).ToString().All(char.IsDigit))
            throw new ArgumentException($"'{value}' is not a valid Open Library {expectedSuffix} identifier.", nameof(value));

        return id;
    }
}
