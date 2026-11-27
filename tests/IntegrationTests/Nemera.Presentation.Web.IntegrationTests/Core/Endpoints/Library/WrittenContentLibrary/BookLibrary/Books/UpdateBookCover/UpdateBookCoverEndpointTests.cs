#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.WrittenContentLibrary.BookLibrary;
using Lumina.Presentation.Web.Common.Exceptions;
using Lumina.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.UpdateBookCover;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Common;
using Lumina.Presentation.Web.Fixtures.Common.DTO.WrittenContentLibrary.BookLibrary;
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
    private readonly ProblemDetailsDtoFixture _problemDetailsDtoFixture = new();
    private readonly UpdateBookCoverDtoFixture _updateBookCoverDtoFixture = new();

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
        Guid libraryId = Guid.NewGuid();
        Guid bookId = Guid.NewGuid();
        string expectedPath = "/media/books/cover.png";
        _apiFactory.ApiClientStub.RegisterPutResponse($"libraries/{libraryId}/books/{bookId}/cover", _updateBookCoverDtoFixture.Create(coverPath: expectedPath));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage uploadRequest = CreateUploadRequest(bookId, libraryId, "cover.png");
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
        Assert.Contains(_apiFactory.ApiClientStub.PutRequests, putRequest => putRequest.Endpoint == $"libraries/{libraryId}/books/{bookId}/cover" && putRequest.Data as string == "cover.png");
    }

    [Fact]
    public async Task UpdateBookCover_WhenRouteIdDiffersFromFormIdField_ShouldUseTheRouteId()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        Guid routeId = Guid.NewGuid();
        Guid formId = Guid.NewGuid();
        _apiFactory.ApiClientStub.RegisterPutResponse($"libraries/{libraryId}/books/{routeId}/cover", _updateBookCoverDtoFixture.Create(coverPath: "/media/books/cover.png"));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage uploadRequest = CreateUploadRequest(routeId, libraryId, "cover.png");
        uploadRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);
        MultipartFormDataContent form = (MultipartFormDataContent)uploadRequest.Content!;
        form.Add(new StringContent(formId.ToString()), "id");

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(uploadRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(_apiFactory.ApiClientStub.PutRequests, putRequest => putRequest.Endpoint == $"libraries/{libraryId}/books/{routeId}/cover");
        Assert.DoesNotContain(_apiFactory.ApiClientStub.PutRequests, putRequest => putRequest.Endpoint == $"libraries/{libraryId}/books/{formId}/cover");
    }

    [Fact]
    public async Task UpdateBookCover_WhenCalledWithoutAntiforgeryToken_ShouldRejectRequest()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        Guid bookId = Guid.NewGuid();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage uploadRequest = CreateUploadRequest(bookId, libraryId, "cover.png");

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(uploadRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.DoesNotContain(_apiFactory.ApiClientStub.PutRequests, putRequest => putRequest.Endpoint == $"libraries/{libraryId}/books/{bookId}/cover");
    }

    [Fact]
    public async Task UpdateBookCover_WhenCalledWithoutUploadedFile_ShouldForwardRequestAndReturnFailure()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        Guid bookId = Guid.NewGuid();
        ProblemDetailsDto problemDetails = _problemDetailsDtoFixture.Create(title: "General.Validation", detail: "BookCoverCannotBeNull");
        _apiFactory.ApiClientStub.RegisterPutException($"libraries/{libraryId}/books/{bookId}/cover", new ApiException(problemDetails, HttpStatusCode.UnprocessableEntity, $"libraries/{libraryId}/books/{bookId}/cover"));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        MultipartFormDataContent noFileForm = [];
        noFileForm.Add(new StringContent("unrelated-field"));
        HttpRequestMessage uploadRequest = new(HttpMethod.Put, $"/en-us/library/written-content-library/books-library/books/{bookId}/api-update-cover?libraryId={libraryId}")
        {
            Content = noFileForm
        };
        uploadRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        uploadRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(uploadRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        // The web does not short-circuit the missing file; it forwards the request, and the API reports the missing cover
        // with the same BookCoverCannotBeNull validation error any other client receives.
        Assert.Contains(_apiFactory.ApiClientStub.PutRequests, putRequest => putRequest.Endpoint == $"libraries/{libraryId}/books/{bookId}/cover" && putRequest.Data == null);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.False(json.RootElement.GetProperty("success").GetBoolean());
    }

    [Fact]
    public async Task UpdateBookCover_WhenNotAuthenticated_ShouldRedirectToLogin()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient anonymousClient = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        HttpRequestMessage uploadRequest = CreateUploadRequest(Guid.NewGuid(), Guid.NewGuid(), "cover.png");

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
    /// <param name="libraryId">The Id of the media library the book belongs to.</param>
    /// <param name="fileName">The name of the uploaded file.</param>
    /// <returns>The configured upload request.</returns>
    private static HttpRequestMessage CreateUploadRequest(Guid bookId, Guid libraryId, string fileName)
    {
        MultipartFormDataContent form = [];
        ByteArrayContent fileContent = new(Encoding.UTF8.GetBytes("fake cover payload"));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        form.Add(fileContent, "cover", fileName);
        return new HttpRequestMessage(HttpMethod.Put, $"/en-us/library/written-content-library/books-library/books/{bookId}/api-update-cover?libraryId={libraryId}")
        {
            Content = form
        };
    }
}
