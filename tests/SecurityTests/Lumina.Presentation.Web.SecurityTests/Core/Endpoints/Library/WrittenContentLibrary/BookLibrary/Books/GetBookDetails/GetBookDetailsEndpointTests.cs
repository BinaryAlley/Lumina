#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.WrittenContentLibrary.BookLibrary;
using Lumina.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.GetBookDetails;
using Lumina.Presentation.Web.Fixtures.Common.DTO.WrittenContentLibrary.BookLibrary;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.SecurityTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.GetBookDetails;

/// <summary>
/// Contains security tests for the <see cref="GetBookDetailsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetBookDetailsEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly BookDetailsDtoFixture _bookDetailsDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetBookDetailsEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetBookDetailsEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetBookDetails_WhenCalledWithoutAuthentication_ShouldRedirectToLoginWithoutCallingApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient anonymousClient = WebTestHelpers.CreateAnonymousClient(_apiFactory);

        // Act
        HttpResponseMessage response = await anonymousClient.GetAsync($"/en-us/library/written-content-library/books-library/books/{Guid.NewGuid()}/api-get-book");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.DoesNotContain(_apiFactory.ApiClientStub.GetEndpointsCalled, endpoint => endpoint.StartsWith("books/", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("'; DROP TABLE Books; --")] // destructive injection
    [InlineData("' OR '1'='1")] // boolean-based injection
    public async Task GetBookDetails_WithSQLInjectionInBookId_ShouldNormalizeTheIdWithoutLeakingData(string maliciousBookId)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        string normalizedBookId = Guid.Empty.ToString();
        BookDetailsDto expectedBook = _bookDetailsDtoFixture.Create();
        _apiFactory.ApiClientStub.RegisterGetResponse($"books/{normalizedBookId}", expectedBook);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync($"/en-us/library/written-content-library/books-library/books/{Uri.EscapeDataString(maliciousBookId)}/api-get-book");
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        // The route Id is normalized to a Guid before it is substituted into the upstream URL, so a crafted route value can never escape
        // its URL segment or reach a different book lookup; the unparseable value is reported by the API as a missing book Id, and the
        // Web layer must not fail local binding, leak stack traces or database details.
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using (JsonDocument json = JsonDocument.Parse(content))
            Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("at Lumina", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(_apiFactory.ApiClientStub.GetEndpointsCalled, endpoint => endpoint == $"books/{normalizedBookId}");
    }
}
