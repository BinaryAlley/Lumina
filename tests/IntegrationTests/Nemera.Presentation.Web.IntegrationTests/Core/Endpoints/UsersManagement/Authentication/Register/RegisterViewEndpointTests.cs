#region ========================================================================= USING =====================================================================================
using Nemera.Presentation.Web.Core.Endpoints.UsersManagement.Authentication.Register;
using Nemera.Presentation.Web.Fixtures.Common.TestHelpers;
using Nemera.Presentation.Web.IntegrationTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Web.IntegrationTests.Core.Endpoints.UsersManagement.Authentication.Register;

/// <summary>
/// Contains integration tests for the <see cref="RegisterViewEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RegisterViewEndpointTests : IClassFixture<NemeraWebFactory>
{
    private readonly NemeraWebFactory _apiFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterViewEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public RegisterViewEndpointTests(NemeraWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task RegisterView_WhenCalled_ShouldRenderRegisterPage()
    {
        // Arrange
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);

        // Act
        HttpResponseMessage response = await client.GetAsync("/en-us/auth/register");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType!.MediaType);
        string content = await response.Content.ReadAsStringAsync();
        Assert.Contains("register-account-form", content);
    }
}
