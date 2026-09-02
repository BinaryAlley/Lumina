#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Reading;
using Lumina.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.Reading.GetReadingSection;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Reading;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.Reading.GetReadingSection;

/// <summary>
/// Contains integration tests for the <c>/{culture}/library/written-content-library/books-library/books/{bookId}/api-get-reading-section</c> route served by the <see cref="GetReadingSectionEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetReadingSectionEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly ReadingSectionDtoFixture _readingSectionDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetReadingSectionEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetReadingSectionEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetReadingSection_WhenCalledByAuthenticatedUser_ShouldReturnSectionJson()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid bookId = Guid.NewGuid();
        string locationRef = "chapter-1";
        ReadingSectionDto section = _readingSectionDtoFixture.Create(locationRef: locationRef, contentHtml: "<p>Content</p>");
        _apiFactory.ApiClientStub.RegisterGetResponse($"books/{bookId}/reading/sections/{locationRef}", section);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = new(HttpMethod.Get, $"/en-us/library/written-content-library/books-library/books/{bookId}/api-get-reading-section?locationRef={locationRef}");
        getRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(locationRef, json.RootElement.GetProperty("data").GetProperty("locationRef").GetString());
    }

    [Fact]
    public async Task GetReadingSection_WhenNotAuthenticated_ShouldRedirectToLogin()
    {
        // Arrange
        HttpClient anonymousClient = WebTestHelpers.CreateAnonymousClient(_apiFactory);

        // Act
        HttpResponseMessage response = await anonymousClient.GetAsync($"/en-us/library/written-content-library/books-library/books/{Guid.NewGuid()}/api-get-reading-section?locationRef=chapter-1");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }
}
