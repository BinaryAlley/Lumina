#region ========================================================================= USING =====================================================================================
using Nemera.Presentation.Web.Core.Endpoints.FileSystemManagement.Path.GetPathSeparator;
using Nemera.Presentation.Web.Fixtures.Common.TestHelpers;
using Nemera.Presentation.Web.SecurityTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
#endregion

namespace Nemera.Presentation.Web.SecurityTests.Core.Endpoints.FileSystemManagement.Path.GetPathSeparator;

/// <summary>
/// Contains security tests for the <see cref="GetPathSeparatorEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPathSeparatorEndpointTests : IClassFixture<NemeraWebFactory>
{
    private readonly NemeraWebFactory _apiFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPathSeparatorEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetPathSeparatorEndpointTests(NemeraWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetPathSeparator_WhenCalledWithoutAuthentication_ShouldRedirectToLogin()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);

        // Act
        HttpResponseMessage response = await client.GetAsync("/path/api-get-path-separator");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost/en-us/auth/login", response.Headers.Location!.ToString());
        Assert.DoesNotContain(_apiFactory.ApiClientStub.GetEndpointsCalled, endpoint => endpoint == "path/get-path-separator");
    }
}
