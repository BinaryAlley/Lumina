#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.FileSystemManagement;
using Lumina.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.Reading.GetReadingResource;
using Lumina.Presentation.Web.Fixtures.Common.DTO.FileSystemManagement;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.Reading.GetReadingResource;

/// <summary>
/// Contains integration tests for the <c>/{culture}/library/written-content-library/books-library/books/{bookId}/api-get-reading-resource</c> route served by the <see cref="GetReadingResourceEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetReadingResourceEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly BlobDataDtoFixture _blobDataDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetReadingResourceEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetReadingResourceEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetReadingResource_WhenCalledByAuthenticatedUser_ShouldReturnResourceBytes()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid bookId = Guid.NewGuid();
        string resourceKey = "cover";
        BlobDataDto blob = _blobDataDtoFixture.Create(contentType: "image/png");
        _apiFactory.ApiClientStub.RegisterBlobResponse(blob);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = new(HttpMethod.Get, $"/en-us/library/written-content-library/books-library/books/{bookId}/api-get-reading-resource?resourceKey={resourceKey}");
        getRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("image/png"));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);
        byte[] body = await response.Content.ReadAsByteArrayAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("image/png", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(blob.Data, body);
    }

    [Fact]
    public async Task GetReadingResource_WhenNotAuthenticated_ShouldRedirectToLogin()
    {
        // Arrange
        HttpClient anonymousClient = WebTestHelpers.CreateAnonymousClient(_apiFactory);

        // Act
        HttpResponseMessage response = await anonymousClient.GetAsync($"/en-us/library/written-content-library/books-library/books/{Guid.NewGuid()}/api-get-reading-resource?resourceKey=cover");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }
}
