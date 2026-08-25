#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Plugins.OpenLibrary.Common.Models.Contracts.Responses;
using Lumina.Plugins.OpenLibrary.Common.Models.DTO.Settings;
using Lumina.Plugins.OpenLibrary.Core.Api;
using Lumina.Plugins.OpenLibrary.Core.Settings;
using Lumina.Plugins.OpenLibrary.Fixtures.Common.Models.DTO.Settings;
using Lumina.Plugins.OpenLibrary.Fixtures.Common.Setup;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Plugins.OpenLibrary.UnitTests.Core.Api;

/// <summary>
/// Contains unit tests for the <see cref="OpenLibraryHttpClient"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class OpenLibraryHttpClientTests
{
    private readonly OpenLibrarySettingsDtoFixture _settingsFixture = new();
    private readonly BookMetadataLookupDtoFixture _bookMetadataLookupDtoFixture = new();

    [Fact]
    public void Constructor_WhenHttpClientHasNoBaseAddress_ShouldSetTheDefaultOpenLibraryBaseAddress()
    {
        // Arrange
        HttpClient httpClient = new(new StubOpenLibraryHttpMessageHandler());

        // Act
        _ = new OpenLibraryHttpClient(httpClient, new OpenLibrarySettingsProvider(null, Guid.NewGuid(), _settingsFixture.Create()));

        // Assert
        Assert.Equal(new Uri("https://openlibrary.org/"), httpClient.BaseAddress);
    }

    [Fact]
    public void Constructor_WhenHttpClientAlreadyHasBaseAddress_ShouldKeepTheExistingBaseAddress()
    {
        // Arrange
        Uri existingBaseAddress = new("https://example.com/");
        HttpClient httpClient = new(new StubOpenLibraryHttpMessageHandler())
        {
            BaseAddress = existingBaseAddress
        };

        // Act
        _ = new OpenLibraryHttpClient(httpClient, new OpenLibrarySettingsProvider(null, Guid.NewGuid(), _settingsFixture.Create()));

        // Assert
        Assert.Equal(existingBaseAddress, httpClient.BaseAddress);
    }

    [Fact]
    public async Task GetEditionAsync_WhenHttpClientHasNoUserAgent_ShouldAddTheConfiguredUserAgent()
    {
        // Arrange
        StubOpenLibraryHttpMessageHandler handler = new();
        handler.MapPath("/books/OL1M.json", """{"key":"/books/OL1M","title":"Edition Title"}""");
        HttpClient httpClient = new(handler);
        OpenLibraryHttpClient sut = new(httpClient, new OpenLibrarySettingsProvider(null, Guid.NewGuid(), _settingsFixture.Create(userAgent: "CustomAgent/2.0")));

        // Act
        await sut.GetEditionAsync("OL1M", CancellationToken.None);

        // Assert
        Assert.Equal("CustomAgent/2.0", httpClient.DefaultRequestHeaders.UserAgent.ToString());
    }

    [Fact]
    public async Task GetEditionAsync_WhenHttpClientAlreadyHasUserAgent_ShouldKeepTheExistingUserAgent()
    {
        // Arrange
        StubOpenLibraryHttpMessageHandler handler = new();
        handler.MapPath("/books/OL1M.json", """{"key":"/books/OL1M","title":"Edition Title"}""");
        HttpClient httpClient = new(handler);
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("ExistingAgent/1.0");
        OpenLibraryHttpClient sut = new(httpClient, new OpenLibrarySettingsProvider(null, Guid.NewGuid(), _settingsFixture.Create(userAgent: "ConfiguredAgent/2.0")));

        // Act
        await sut.GetEditionAsync("OL1M", CancellationToken.None);

        // Assert
        Assert.Equal("ExistingAgent/1.0", httpClient.DefaultRequestHeaders.UserAgent.ToString());
    }

    [Fact]
    public async Task GetEditionAsync_WhenContactEmailIsConfigured_ShouldSetTheFromHeader()
    {
        // Arrange
        StubOpenLibraryHttpMessageHandler handler = new();
        handler.MapPath("/books/OL1M.json", """{"key":"/books/OL1M","title":"Edition Title"}""");
        HttpClient httpClient = new(handler);
        OpenLibraryHttpClient sut = new(httpClient, new OpenLibrarySettingsProvider(null, Guid.NewGuid(), _settingsFixture.Create(contactEmail: "contact@example.com")));

        // Act
        await sut.GetEditionAsync("OL1M", CancellationToken.None);

        // Assert
        Assert.Equal("contact@example.com", httpClient.DefaultRequestHeaders.From);
    }

    [Fact]
    public async Task GetEditionAsync_WhenContactEmailIsNotConfigured_ShouldNotSetTheFromHeader()
    {
        // Arrange
        StubOpenLibraryHttpMessageHandler handler = new();
        handler.MapPath("/books/OL1M.json", """{"key":"/books/OL1M","title":"Edition Title"}""");
        HttpClient httpClient = new(handler);
        OpenLibraryHttpClient sut = new(httpClient, new OpenLibrarySettingsProvider(null, Guid.NewGuid(), _settingsFixture.Create(contactEmail: null)));

        // Act
        await sut.GetEditionAsync("OL1M", CancellationToken.None);

        // Assert
        Assert.Null(httpClient.DefaultRequestHeaders.From);
    }

    [Fact]
    public async Task GetEditionByIsbnAsync_WhenEditionExists_ShouldRequestTheNormalizedIsbnEndpointAndDeserialize()
    {
        // Arrange
        StubOpenLibraryHttpMessageHandler handler = new();
        handler.MapPath("/isbn/9780306406157.json", """{"key":"/books/OL1M","title":"Edition Title"}""");
        OpenLibraryHttpClient sut = CreateSut(handler);

        // Act
        OpenLibraryEditionResponse? result = await sut.GetEditionByIsbnAsync("978-0-306-40615-7", CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Edition Title", result!.Title);
        Assert.Equal("/isbn/9780306406157.json", handler.Requests[0].RequestUri!.AbsolutePath);
    }

    [Fact]
    public async Task GetEditionByIsbnAsync_WhenEditionIsNotFound_ShouldReturnNull()
    {
        // Arrange
        StubOpenLibraryHttpMessageHandler handler = new();
        OpenLibraryHttpClient sut = CreateSut(handler);

        // Act
        OpenLibraryEditionResponse? result = await sut.GetEditionByIsbnAsync("978-0-306-40615-7", CancellationToken.None);

        // Assert
        Assert.Null(result);
        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task GetEditionAsync_WhenCalled_ShouldRequestTheBooksEndpointWithTheNormalizedOlid()
    {
        // Arrange
        StubOpenLibraryHttpMessageHandler handler = new();
        handler.MapPath("/books/OL123M.json", """{"key":"/books/OL123M","title":"Edition"}""");
        OpenLibraryHttpClient sut = CreateSut(handler);

        // Act
        OpenLibraryEditionResponse? result = await sut.GetEditionAsync("/books/ol123m", CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("/books/OL123M.json", handler.Requests[0].RequestUri!.AbsolutePath);
        Assert.Equal("/books/OL123M", result!.Key);
    }

    [Fact]
    public async Task GetEditionAsync_WhenOlidIsNotAnEditionId_ShouldThrowArgumentException()
    {
        // Arrange
        OpenLibraryHttpClient sut = CreateSut(new StubOpenLibraryHttpMessageHandler());

        // Act
        async Task Act()
        {
            await sut.GetEditionAsync("OL123W", CancellationToken.None);
        }

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(Act);
    }

    [Fact]
    public async Task GetWorkAsync_WhenCalled_ShouldRequestTheWorksEndpointWithTheNormalizedOlid()
    {
        // Arrange
        StubOpenLibraryHttpMessageHandler handler = new();
        handler.MapPath("/works/OL123W.json", """{"key":"/works/OL123W","title":"Work"}""");
        OpenLibraryHttpClient sut = CreateSut(handler);

        // Act
        OpenLibraryWorkResponse? result = await sut.GetWorkAsync("OL123W", CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("/works/OL123W.json", handler.Requests[0].RequestUri!.AbsolutePath);
        Assert.Equal("Work", result!.Title);
    }

    [Fact]
    public async Task GetAuthorAsync_WhenCalled_ShouldRequestTheAuthorsEndpointWithTheNormalizedOlid()
    {
        // Arrange
        StubOpenLibraryHttpMessageHandler handler = new();
        handler.MapPath("/authors/OL1A.json", """{"key":"/authors/OL1A","name":"Test Author"}""");
        OpenLibraryHttpClient sut = CreateSut(handler);

        // Act
        OpenLibraryAuthorResponse? result = await sut.GetAuthorAsync("OL1A", CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("/authors/OL1A.json", handler.Requests[0].RequestUri!.AbsolutePath);
        Assert.Equal("Test Author", result!.Name);
    }

    [Fact]
    public async Task GetRatingsAsync_WhenCalled_ShouldRequestTheRatingsEndpoint()
    {
        // Arrange
        StubOpenLibraryHttpMessageHandler handler = new();
        handler.MapPath("/works/OL123W/ratings.json", """{"summary":{"average":4.2,"count":100}}""");
        OpenLibraryHttpClient sut = CreateSut(handler);

        // Act
        OpenLibraryRatingsResponse? result = await sut.GetRatingsAsync("OL123W", CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("/works/OL123W/ratings.json", handler.Requests[0].RequestUri!.AbsolutePath);
        Assert.Equal(4.2m, result!.Summary!.Average);
        Assert.Equal(100, result.Summary.Count);
    }

    [Fact]
    public async Task GetRatingsAsync_WhenNumbersAreSerializedAsStrings_ShouldStillDeserializeThem()
    {
        // Arrange
        StubOpenLibraryHttpMessageHandler handler = new();
        handler.MapPath("/works/OL123W/ratings.json", """{"summary":{"average":"4.2","count":"100"}}""");
        OpenLibraryHttpClient sut = CreateSut(handler);

        // Act
        OpenLibraryRatingsResponse? result = await sut.GetRatingsAsync("OL123W", CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(4.2m, result!.Summary!.Average);
        Assert.Equal(100, result.Summary.Count);
    }

    [Fact]
    public async Task GetEditionsAsync_WhenCalled_ShouldRequestTheEditionsEndpointWithTheConfiguredLimit()
    {
        // Arrange
        StubOpenLibraryHttpMessageHandler handler = new();
        handler.MapPath("/works/OL123W/editions.json", """{"entries":[{"key":"/books/OL1M","title":"Edition"}]}""");
        OpenLibraryHttpClient sut = CreateSut(handler);

        // Act
        IReadOnlyList<OpenLibraryEditionResponse> result = await sut.GetEditionsAsync("OL123W", 50, CancellationToken.None);

        // Assert
        OpenLibraryEditionResponse edition = Assert.Single(result);
        Assert.Equal("Edition", edition.Title);
        Assert.Equal("/works/OL123W/editions.json", handler.Requests[0].RequestUri!.AbsolutePath);
        Assert.Contains("limit=50", handler.Requests[0].RequestUri!.Query);
    }

    [Theory]
    [InlineData(0, "limit=1")]
    [InlineData(1, "limit=1")]
    [InlineData(1000, "limit=1000")]
    [InlineData(5000, "limit=1000")]
    public async Task GetEditionsAsync_WhenLimitIsOutsideTheAllowedRange_ShouldClampTheLimit(int limit, string expectedQuery)
    {
        // Arrange
        StubOpenLibraryHttpMessageHandler handler = new();
        handler.MapPath("/works/OL123W/editions.json", """{"entries":[]}""");
        OpenLibraryHttpClient sut = CreateSut(handler);

        // Act
        await sut.GetEditionsAsync("OL123W", limit, CancellationToken.None);

        // Assert
        Assert.Contains(expectedQuery, handler.Requests[0].RequestUri!.Query);
    }

    [Fact]
    public async Task GetEditionsAsync_WhenResponseHasNoEntries_ShouldReturnAnEmptyList()
    {
        // Arrange
        StubOpenLibraryHttpMessageHandler handler = new();
        handler.MapPath("/works/OL123W/editions.json", """{"entries":[]}""");
        OpenLibraryHttpClient sut = CreateSut(handler);

        // Act
        IReadOnlyList<OpenLibraryEditionResponse> result = await sut.GetEditionsAsync("OL123W", 50, CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetEditionsAsync_WhenEndpointReturnsNotFound_ShouldReturnAnEmptyList()
    {
        // Arrange
        StubOpenLibraryHttpMessageHandler handler = new();
        OpenLibraryHttpClient sut = CreateSut(handler);

        // Act
        IReadOnlyList<OpenLibraryEditionResponse> result = await sut.GetEditionsAsync("OL123W", 50, CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task SearchAsync_WhenLookupContainsAllCriteria_ShouldBuildTheQueryWithAllParameters()
    {
        // Arrange
        StubOpenLibraryHttpMessageHandler handler = new();
        handler.MapPath("/search.json", """{"docs":[]}""");
        OpenLibraryHttpClient sut = CreateSut(handler);
        BookMetadataLookupDto lookup = _bookMetadataLookupDtoFixture.Create(
            isbn: "978-0-306-40615-7",
            title: "The Title",
            author: "The Author",
            languageCode: "en");

        // Act
        IReadOnlyList<OpenLibrarySearchDocumentResponse> result = await sut.SearchAsync(lookup, 5, CancellationToken.None);

        // Assert
        Assert.Empty(result);
        Assert.Equal("/search.json", handler.Requests[0].RequestUri!.AbsolutePath);
        string query = handler.Requests[0].RequestUri!.Query;
        Assert.Contains("isbn=9780306406157", query);
        Assert.Contains("title=The%20Title", query);
        Assert.Contains("author=The%20Author", query);
        Assert.Contains("language=en", query);
        Assert.Contains("fields=", query);
        Assert.Contains("limit=5", query);
    }

    [Fact]
    public async Task SearchAsync_WhenLookupOnlyHasATitle_ShouldBuildATitleOnlyQuery()
    {
        // Arrange
        StubOpenLibraryHttpMessageHandler handler = new();
        handler.MapPath("/search.json", """{"docs":[]}""");
        OpenLibraryHttpClient sut = CreateSut(handler);
        BookMetadataLookupDto lookup = _bookMetadataLookupDtoFixture.Create(path: "/books/title.epub", title: "Dune");

        // Act
        await sut.SearchAsync(lookup, 5, CancellationToken.None);

        // Assert
        string query = handler.Requests[0].RequestUri!.Query;
        Assert.Contains("title=Dune", query);
        Assert.DoesNotContain("isbn=", query);
        Assert.DoesNotContain("author=", query);
        Assert.DoesNotContain("language=", query);
    }

    [Theory]
    [InlineData(0, "limit=1")]
    [InlineData(100, "limit=100")]
    [InlineData(1000, "limit=100")]
    public async Task SearchAsync_WhenLimitIsOutsideTheAllowedRange_ShouldClampTheLimit(int limit, string expectedQuery)
    {
        // Arrange
        StubOpenLibraryHttpMessageHandler handler = new();
        handler.MapPath("/search.json", """{"docs":[]}""");
        OpenLibraryHttpClient sut = CreateSut(handler);
        BookMetadataLookupDto lookup = _bookMetadataLookupDtoFixture.Create(path: "/books/title.epub", title: "Dune");

        // Act
        await sut.SearchAsync(lookup, limit, CancellationToken.None);

        // Assert
        Assert.Contains(expectedQuery, handler.Requests[0].RequestUri!.Query);
    }

    [Fact]
    public async Task SearchAsync_WhenResponseContainsDocuments_ShouldReturnTheDocuments()
    {
        // Arrange
        StubOpenLibraryHttpMessageHandler handler = new();
        handler.MapPath("/search.json", """{"docs":[{"key":"/works/OL1W","title":"Doc Title"}]}""");
        OpenLibraryHttpClient sut = CreateSut(handler);
        BookMetadataLookupDto lookup = _bookMetadataLookupDtoFixture.Create(path: "/books/title.epub", title: "Dune");

        // Act
        IReadOnlyList<OpenLibrarySearchDocumentResponse> result = await sut.SearchAsync(lookup, 5, CancellationToken.None);

        // Assert
        OpenLibrarySearchDocumentResponse document = Assert.Single(result);
        Assert.Equal("Doc Title", document.Title);
    }

    [Fact]
    public async Task SearchAsync_WhenEndpointReturnsNotFound_ShouldReturnAnEmptyList()
    {
        // Arrange
        StubOpenLibraryHttpMessageHandler handler = new();
        OpenLibraryHttpClient sut = CreateSut(handler);
        BookMetadataLookupDto lookup = _bookMetadataLookupDtoFixture.Create(path: "/books/title.epub", title: "Dune");

        // Act
        IReadOnlyList<OpenLibrarySearchDocumentResponse> result = await sut.SearchAsync(lookup, 5, CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetWorkAsync_WhenFirstAttemptFailsWithServerError_ShouldRetryAndSucceed()
    {
        // Arrange
        int attempts = 0;
        StubOpenLibraryHttpMessageHandler handler = new();
        handler.AddRoute(
            request => request.RequestUri!.AbsolutePath == "/works/OL123W.json",
            _ => attempts++ == 0
                ? new HttpResponseMessage(HttpStatusCode.InternalServerError)
                : StubOpenLibraryHttpMessageHandler.CreateJsonResponse(HttpStatusCode.OK, """{"key":"/works/OL123W","title":"Work"}"""));
        OpenLibraryHttpClient sut = CreateSut(handler, _settingsFixture.Create(minimumRequestInterval: TimeSpan.Zero));

        // Act
        OpenLibraryWorkResponse? result = await sut.GetWorkAsync("OL123W", CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Work", result!.Title);
        Assert.Equal(2, handler.Requests.Count);
    }

    [Fact]
    public async Task GetWorkAsync_WhenFirstAttemptFailsWithTooManyRequests_ShouldRetryAfterTheRetryAfterDelayAndSucceed()
    {
        // Arrange
        int attempts = 0;
        StubOpenLibraryHttpMessageHandler handler = new();
        handler.AddRoute(
            request => request.RequestUri!.AbsolutePath == "/works/OL123W.json",
            _ =>
            {
                if (attempts++ == 0)
                {
                    HttpResponseMessage tooManyRequests = new(HttpStatusCode.TooManyRequests);
                    tooManyRequests.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromMilliseconds(1));
                    return tooManyRequests;
                }

                return StubOpenLibraryHttpMessageHandler.CreateJsonResponse(HttpStatusCode.OK, """{"key":"/works/OL123W","title":"Work"}""");
            });
        OpenLibraryHttpClient sut = CreateSut(handler, _settingsFixture.Create(minimumRequestInterval: TimeSpan.Zero));

        // Act
        OpenLibraryWorkResponse? result = await sut.GetWorkAsync("OL123W", CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Work", result!.Title);
        Assert.Equal(2, handler.Requests.Count);
    }

    [Fact]
    public async Task GetWorkAsync_WhenEveryAttemptFailsWithServerError_ShouldThrowHttpRequestException()
    {
        // Arrange
        StubOpenLibraryHttpMessageHandler handler = new();
        handler.AddRoute(
            request => request.RequestUri!.AbsolutePath == "/works/OL123W.json",
            _ => new HttpResponseMessage(HttpStatusCode.InternalServerError));
        OpenLibraryHttpClient sut = CreateSut(handler, _settingsFixture.Create(minimumRequestInterval: TimeSpan.Zero));

        // Act
        async Task Act()
        {
            await sut.GetWorkAsync("OL123W", CancellationToken.None);
        }

        // Assert
        await Assert.ThrowsAsync<HttpRequestException>(Act);
        Assert.Equal(3, handler.Requests.Count);
    }

    [Fact]
    public async Task GetWorkAsync_WhenEndpointReturnsBadRequest_ShouldThrowImmediately()
    {
        // Arrange
        StubOpenLibraryHttpMessageHandler handler = new();
        handler.AddRoute(
            request => request.RequestUri!.AbsolutePath == "/works/OL123W.json",
            _ => new HttpResponseMessage(HttpStatusCode.BadRequest));
        OpenLibraryHttpClient sut = CreateSut(handler, _settingsFixture.Create(minimumRequestInterval: TimeSpan.Zero));

        // Act
        async Task Act()
        {
            await sut.GetWorkAsync("OL123W", CancellationToken.None);
        }

        // Assert
        await Assert.ThrowsAsync<HttpRequestException>(Act);
        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task GetWorkAsync_WhenRequestsAreCloserThanTheMinimumInterval_ShouldDelayTheSecondRequest()
    {
        // Arrange
        StubOpenLibraryHttpMessageHandler handler = new();
        handler.MapPath("/works/OL1W.json", """{"key":"/works/OL1W","title":"Work"}""");
        OpenLibraryHttpClient sut = CreateSut(handler, _settingsFixture.Create(minimumRequestInterval: TimeSpan.FromMilliseconds(300)));

        // Act
        Stopwatch stopwatch = Stopwatch.StartNew();
        await sut.GetWorkAsync("OL1W", CancellationToken.None);
        await sut.GetWorkAsync("OL1W", CancellationToken.None);
        stopwatch.Stop();

        // Assert
        Assert.True(stopwatch.ElapsedMilliseconds >= 200, $"Expected the two requests to take at least 200ms, but they took {stopwatch.ElapsedMilliseconds}ms.");
    }

    [Fact]
    public async Task GetWorkAsync_WhenCancellationIsRequested_ShouldCancelTheOperation()
    {
        // Arrange
        OpenLibraryHttpClient sut = CreateSut(new StubOpenLibraryHttpMessageHandler());
        using CancellationTokenSource cancellationTokenSource = new();
        cancellationTokenSource.Cancel();

        // Act
        async Task Act()
        {
            await sut.GetWorkAsync("OL123W", cancellationTokenSource.Token);
        }

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(Act);
    }

    /// <summary>
    /// Creates the Open Library HTTP client under test backed by the given handler and settings.
    /// </summary>
    /// <param name="handler">The stub HTTP message handler used to serve responses.</param>
    /// <param name="settings">Optional. The Open Library settings used by the client. When omitted, randomized settings are used.</param>
    /// <returns>The created <see cref="OpenLibraryHttpClient"/>.</returns>
    private OpenLibraryHttpClient CreateSut(StubOpenLibraryHttpMessageHandler handler, OpenLibrarySettingsDto? settings = null)
    {
        HttpClient httpClient = new(handler);
        OpenLibrarySettingsProvider settingsProvider = new(null, Guid.NewGuid(), settings ?? _settingsFixture.Create(minimumRequestInterval: TimeSpan.Zero));
        return new OpenLibraryHttpClient(httpClient, settingsProvider);
    }
}
