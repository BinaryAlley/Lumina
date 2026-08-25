#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Api.SecurityTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.Maintenance.ApplicationSetup;

/// <summary>
/// Contains security tests for the <c>/initialization</c> route.
/// </summary>
[ExcludeFromCodeCoverage]
public class CheckInitializationEndpointTests : IClassFixture<LuminaApiFactory>
{
    private readonly HttpClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="CheckInitializationEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public CheckInitializationEndpointTests(LuminaApiFactory apiFactory)
    {
        _client = apiFactory.CreateClient();
    }

    [Fact]
    public async Task CheckInitialization_WhenCalledWithoutAuthentication_ShouldBeAccessible()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/v1/initialization");

        // Assert
        // the initialization check is anonymous, so it must never demand authentication or leak any internal data
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("password", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("salt", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
    }
}
