#region ========================================================================= USING =====================================================================================
using Nemera.Presentation.Web.Core.Endpoints.Tools.Settings;
using Nemera.Presentation.Web.Fixtures.Common.TestHelpers;
using Nemera.Presentation.Web.SecurityTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
#endregion

namespace Nemera.Presentation.Web.SecurityTests.Core.Endpoints.Tools.Settings;

/// <summary>
/// Contains security tests for the <see cref="SettingsViewEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SettingsViewEndpointTests : IClassFixture<NemeraWebFactory>
{
    private readonly NemeraWebFactory _apiFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsViewEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public SettingsViewEndpointTests(NemeraWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task SettingsView_WhenCalledWithoutAuthentication_ShouldRedirectToLogin()
    {
        // Arrange
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);

        // Act
        HttpResponseMessage response = await client.GetAsync("/en-us/tools/settings");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost/en-us/auth/login", response.Headers.Location!.ToString());
    }
}
