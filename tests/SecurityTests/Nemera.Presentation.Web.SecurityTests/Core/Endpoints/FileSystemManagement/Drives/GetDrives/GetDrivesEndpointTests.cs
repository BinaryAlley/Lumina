#region ========================================================================= USING =====================================================================================
using Nemera.Presentation.Web.Core.Endpoints.FileSystemManagement.Drives.GetDrives;
using Nemera.Presentation.Web.Fixtures.Common.TestHelpers;
using Nemera.Presentation.Web.SecurityTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
#endregion

namespace Nemera.Presentation.Web.SecurityTests.Core.Endpoints.FileSystemManagement.Drives.GetDrives;

/// <summary>
/// Contains security tests for the <see cref="GetDrivesEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetDrivesEndpointTests : IClassFixture<NemeraWebFactory>
{
    private readonly NemeraWebFactory _apiFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDrivesEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetDrivesEndpointTests(NemeraWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetDrives_WhenCalledWithoutAuthentication_ShouldRedirectToLogin()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);

        // Act
        HttpResponseMessage response = await client.GetAsync("/drives/api-get-drives");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost/en-us/auth/login", response.Headers.Location!.ToString());
        Assert.DoesNotContain(_apiFactory.ApiClientStub.GetEndpointsCalled, endpoint => endpoint == "drives/get-drives");
    }
}
