#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Reading;
using Lumina.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.Reading.GetReadingAvailability;
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

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.Reading.GetReadingAvailability;

/// <summary>
/// Contains integration tests for the <c>/{culture}/library/written-content-library/books-library/books/{bookId}/api-get-reading-availability</c> route served by the <see cref="GetReadingAvailabilityEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetReadingAvailabilityEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly ReadingAvailabilityDtoFixture _readingAvailabilityDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetReadingAvailabilityEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetReadingAvailabilityEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetReadingAvailability_WhenCalledByAuthenticatedUser_ShouldReturnAvailabilityJson()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid bookId = Guid.NewGuid();
        ReadingAvailabilityDto availability = _readingAvailabilityDtoFixture.Create(bookId: bookId, isAvailable: true);
        _apiFactory.ApiClientStub.RegisterGetResponse($"books/{bookId}/reading/availability", availability);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = new(HttpMethod.Get, $"/en-us/library/written-content-library/books-library/books/{bookId}/api-get-reading-availability");
        getRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(availability.LibraryId, json.RootElement.GetProperty("libraryId").GetGuid());
    }

    [Fact]
    public async Task GetReadingAvailability_WhenCalled_ShouldRequestAvailabilityFromApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid bookId = Guid.NewGuid();
        _apiFactory.ApiClientStub.RegisterGetResponse($"books/{bookId}/reading/availability", _readingAvailabilityDtoFixture.Create(bookId: bookId));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = new(HttpMethod.Get, $"/en-us/library/written-content-library/books-library/books/{bookId}/api-get-reading-availability");
        getRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Contains($"books/{bookId}/reading/availability", _apiFactory.ApiClientStub.GetEndpointsCalled);
    }

    [Fact]
    public async Task GetReadingAvailability_WhenNotAuthenticated_ShouldRedirectToLogin()
    {
        // Arrange
        HttpClient anonymousClient = WebTestHelpers.CreateAnonymousClient(_apiFactory);

        // Act
        HttpResponseMessage response = await anonymousClient.GetAsync($"/en-us/library/written-content-library/books-library/books/{Guid.NewGuid()}/api-get-reading-availability");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }
}
