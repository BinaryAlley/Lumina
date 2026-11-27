#region ========================================================================= USING =====================================================================================
using Nemera.Presentation.Web.Core.Endpoints.Library.Management.GetLibraries;
using Nemera.Presentation.Web.Fixtures.Common.TestHelpers;
using Nemera.Presentation.Web.SecurityTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
#endregion

namespace Nemera.Presentation.Web.SecurityTests.Core.Endpoints.Library.Management.GetLibraries;

/// <summary>
/// Contains security tests for the <see cref="GetLibrariesEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibrariesEndpointTests : IClassFixture<NemeraWebFactory>
{
    private readonly NemeraWebFactory _apiFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibrariesEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetLibrariesEndpointTests(NemeraWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetLibraries_WhenCalledWithoutAuthentication_ShouldRedirectToLogin()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        HttpRequestMessage getRequest = new(HttpMethod.Get, "/en-us/libraries/manage/api-get-libraries");
        getRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await client.SendAsync(getRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost/en-us/auth/login", response.Headers.Location!.ToString());
        Assert.DoesNotContain(_apiFactory.ApiClientStub.GetEndpointsCalled, endpoint => endpoint == "libraries");
    }
}
