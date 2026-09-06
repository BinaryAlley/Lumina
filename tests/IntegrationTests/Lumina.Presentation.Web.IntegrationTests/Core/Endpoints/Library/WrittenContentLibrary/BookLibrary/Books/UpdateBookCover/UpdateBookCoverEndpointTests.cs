#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.UpdateBookCover;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.UpdateBookCover;

/// <summary>
/// Contains integration tests for the <see cref="UpdateBookCoverEndpoint"/> class.
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

    [Fact]
    public async Task UpdateBookCover_WhenCalledByAuthenticatedUserWithAntiforgeryToken_ShouldUploadCoverAndReturnSuccess()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid bookId = Guid.NewGuid();
        string expectedPath = "/media/books/cover.png";
        _apiFactory.ApiClientStub.RegisterPutResponse($"books/{bookId}/cover", expectedPath);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage uploadRequest = CreateUploadRequest(bookId, "cover.png");
        uploadRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(uploadRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(expectedPath, json.RootElement.GetProperty("data").GetString());
        Assert.Contains(_apiFactory.ApiClientStub.PutRequests, putRequest => putRequest.Endpoint == $"books/{bookId}/cover" && putRequest.Data as string == "cover.png");
    }

    [Fact]
    public async Task UpdateBookCover_WhenRouteIdDiffersFromFormIdField_ShouldUseTheRouteId()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid routeId = Guid.NewGuid();
        Guid formId = Guid.NewGuid();
        _apiFactory.ApiClientStub.RegisterPutResponse($"books/{routeId}/cover", "/media/books/cover.png");
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage uploadRequest = CreateUploadRequest(routeId, "cover.png");
        uploadRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);
        MultipartFormDataContent form = (MultipartFormDataContent)uploadRequest.Content!;
        form.Add(new StringContent(formId.ToString()), "id");

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(uploadRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(_apiFactory.ApiClientStub.PutRequests, putRequest => putRequest.Endpoint == $"books/{routeId}/cover");
        Assert.DoesNotContain(_apiFactory.ApiClientStub.PutRequests, putRequest => putRequest.Endpoint == $"books/{formId}/cover");
    }

    [Fact]
    public async Task UpdateBookCover_WhenCalledWithoutAntiforgeryToken_ShouldRejectRequest()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid bookId = Guid.NewGuid();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage uploadRequest = CreateUploadRequest(bookId, "cover.png");

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(uploadRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.DoesNotContain(_apiFactory.ApiClientStub.PutRequests, putRequest => putRequest.Endpoint == $"books/{bookId}/cover");
    }

    [Fact]
    public async Task UpdateBookCover_WhenCalledWithoutUploadedFile_ShouldReturnBadRequest()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid bookId = Guid.NewGuid();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        MultipartFormDataContent noFileForm = [];
        noFileForm.Add(new StringContent("unrelated-field"));
        HttpRequestMessage uploadRequest = new(HttpMethod.Put, $"/en-us/library/written-content-library/books-library/books/{bookId}/api-update-cover")
        {
            Content = noFileForm
        };
        uploadRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        uploadRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(uploadRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.DoesNotContain(_apiFactory.ApiClientStub.PutRequests, putRequest => putRequest.Endpoint == $"books/{bookId}/cover");
    }

    [Fact]
    public async Task UpdateBookCover_WhenNotAuthenticated_ShouldRedirectToLogin()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient anonymousClient = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        HttpRequestMessage uploadRequest = CreateUploadRequest(Guid.NewGuid(), "cover.png");

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
    private static HttpRequestMessage CreateUploadRequest(Guid bookId, string fileName)
    {
        MultipartFormDataContent form = [];
        ByteArrayContent fileContent = new(Encoding.UTF8.GetBytes("fake cover payload"));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        form.Add(fileContent, "cover", fileName);
        return new HttpRequestMessage(HttpMethod.Put, $"/en-us/library/written-content-library/books-library/books/{bookId}/api-update-cover")
        {
            Content = form
        };
    }
}
