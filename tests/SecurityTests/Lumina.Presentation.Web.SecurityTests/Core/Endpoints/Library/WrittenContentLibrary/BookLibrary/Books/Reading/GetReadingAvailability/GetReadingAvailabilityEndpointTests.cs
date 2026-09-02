#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Reading;
using Lumina.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.Reading.GetReadingAvailability;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Reading;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.SecurityTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.Reading.GetReadingAvailability;

/// <summary>
/// Contains security tests for the <c>/{culture}/library/written-content-library/books-library/books/{bookId}/api-get-reading-availability</c> route served by the <see cref="GetReadingAvailabilityEndpoint"/> class.
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
    public async Task GetReadingAvailability_WhenCalledWithoutAuthentication_ShouldRedirectToLogin()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);

        // Act
        HttpResponseMessage response = await client.GetAsync($"/en-us/library/written-content-library/books-library/books/{Guid.NewGuid()}/api-get-reading-availability");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost/en-us/auth/login", response.Headers.Location!.ToString());
        Assert.DoesNotContain(_apiFactory.ApiClientStub.GetEndpointsCalled, endpoint => endpoint.Contains("reading/availability", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("' OR '1'='1")] // basic SQL injection
    [InlineData("'; DROP TABLE Books--")] // destructive injection
    public async Task GetReadingAvailability_WhenCalledWithInjectionInBookId_ShouldRemainSecure(string maliciousBookId)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.RegisterGetResponse($"books/{maliciousBookId}/reading/availability", _readingAvailabilityDtoFixture.Create());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = new(HttpMethod.Get, $"/en-us/library/written-content-library/books-library/books/{Uri.EscapeDataString(maliciousBookId)}/api-get-reading-availability");
        getRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.DoesNotContain("SQL", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("stack trace", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("salt", content, StringComparison.OrdinalIgnoreCase);
    }
}
