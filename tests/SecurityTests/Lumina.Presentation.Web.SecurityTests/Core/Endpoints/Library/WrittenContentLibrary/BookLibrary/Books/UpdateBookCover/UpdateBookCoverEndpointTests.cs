#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.UpdateBookCover;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.SecurityTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.UpdateBookCover;

/// <summary>
/// Contains security tests for the <see cref="UpdateBookCoverEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateBookCoverEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateBookCoverEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public UpdateBookCoverEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Theory]
    [InlineData("'; DROP TABLE Books; --")] // destructive injection
    [InlineData("' OR '1'='1")] // boolean-based injection
    public async Task UpdateBookCover_WithSQLInjectionInRouteId_ShouldNeverReachABookOrLeakData(string maliciousBookId)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.RegisterPutResponse($"books/{Guid.Empty}/cover", "/media/books/cover.png");
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage uploadRequest = CreateUploadRequest(maliciousBookId, "cover.png");
        uploadRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(uploadRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        // The route is authoritative: an unparseable route value becomes an empty book Id, so the upload can never reach a real book.
        Assert.DoesNotContain("SQL", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(_apiFactory.ApiClientStub.PutRequests, putRequest => putRequest.Endpoint == $"books/{Guid.Empty}/cover");
    }

    [Fact]
    public async Task UpdateBookCover_WithPathTraversalFileName_ShouldNotLeakData()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.RegisterPutResponse($"books/{Guid.NewGuid()}/cover", "/media/books/cover.png");
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage uploadRequest = CreateUploadRequest(Guid.NewGuid().ToString(), "..\\..\\evil.png");
        uploadRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(uploadRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("..\\", content, StringComparison.Ordinal);
    }

    [Fact]
    public async Task UpdateBookCover_WhenCalledWithoutAuthentication_ShouldRedirectToLogin()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient anonymousClient = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        HttpRequestMessage uploadRequest = CreateUploadRequest(Guid.NewGuid().ToString(), "cover.png");

        // Act
        HttpResponseMessage response = await anonymousClient.SendAsync(uploadRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Empty(_apiFactory.ApiClientStub.PutRequests);
    }

    /// <summary>
    /// Builds the multipart PUT request that uploads a cover to the book identified by <paramref name="bookId"/>.
    /// </summary>
    /// <param name="bookId">The route Id of the book whose cover is uploaded.</param>
    /// <param name="fileName">The name of the uploaded file.</param>
    /// <returns>The configured upload request.</returns>
    private static HttpRequestMessage CreateUploadRequest(string bookId, string fileName)
    {
        MultipartFormDataContent form = [];
        ByteArrayContent fileContent = new(Encoding.UTF8.GetBytes("fake cover payload"));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        form.Add(fileContent, "cover", fileName);
        return new HttpRequestMessage(HttpMethod.Put, $"/en-us/library/written-content-library/books-library/books/{Uri.EscapeDataString(bookId)}/api-update-cover")
        {
            Content = form
        };
    }
}
