#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books.Reading;
using Lumina.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.Reading.Read;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books.Reading;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.Reading.Read;

/// <summary>
/// Contains integration tests for the <c>/{culture}/library/written-content-library/books-library/books/{bookId}/read</c> route served by the <see cref="ReadViewEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReadViewEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly ReadBookViewRequestFixture _readBookViewRequestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ReadViewEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public ReadViewEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task Read_WhenCalledByAuthenticatedUser_ShouldRenderTheReadView()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        ReadBookViewRequest request = _readBookViewRequestFixture.Create();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync($"/en-us/library/written-content-library/books-library/books/{request.BookId}/read");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("text/html", response.Content.Headers.ContentType?.MediaType, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Read_WhenNotAuthenticated_ShouldRedirectToLogin()
    {
        // Arrange
        HttpClient anonymousClient = WebTestHelpers.CreateAnonymousClient(_apiFactory);

        // Act
        HttpResponseMessage response = await anonymousClient.GetAsync($"/en-us/library/written-content-library/books-library/books/{Guid.NewGuid()}/read");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }
}
