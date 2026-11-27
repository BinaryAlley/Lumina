#region ========================================================================= USING =====================================================================================
using Nemera.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.Reading.Read;
using Nemera.Presentation.Web.Fixtures.Common.TestHelpers;
using Nemera.Presentation.Web.SecurityTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Web.SecurityTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.Reading.Read;

/// <summary>
/// Contains security tests for the <see cref="ReadViewEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReadViewEndpointTests : IClassFixture<NemeraWebFactory>
{
    private readonly NemeraWebFactory _apiFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReadViewEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public ReadViewEndpointTests(NemeraWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task Read_WhenCalledWithoutAuthentication_ShouldRedirectToLogin()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);

        // Act
        HttpResponseMessage response = await client.GetAsync($"/en-us/library/written-content-library/books-library/books/{Guid.NewGuid()}/read");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost/en-us/auth/login", response.Headers.Location!.ToString());
    }
}
