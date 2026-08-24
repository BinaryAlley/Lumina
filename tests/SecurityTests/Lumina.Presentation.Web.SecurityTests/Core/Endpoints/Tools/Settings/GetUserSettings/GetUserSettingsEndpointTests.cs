#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Core.Endpoints.Tools.Settings.GetUserSettings;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.SecurityTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
#endregion

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.Tools.Settings.GetUserSettings;

/// <summary>
/// Contains security tests for the <c>/{culture}/tools/settings/api-get-user-settings</c> route served by the <see cref="GetUserSettingsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetUserSettingsEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserSettingsEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetUserSettingsEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetUserSettings_WhenCalledWithoutAuthentication_ShouldRedirectToLogin()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        HttpRequestMessage getRequest = new(HttpMethod.Get, "/en-us/tools/settings/api-get-user-settings");
        getRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await client.SendAsync(getRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost/en-us/auth/login", response.Headers.Location!.ToString());
        Assert.DoesNotContain(_apiFactory.ApiClientStub.GetEndpointsCalled, endpoint => endpoint == "users/me/settings");
    }
}
