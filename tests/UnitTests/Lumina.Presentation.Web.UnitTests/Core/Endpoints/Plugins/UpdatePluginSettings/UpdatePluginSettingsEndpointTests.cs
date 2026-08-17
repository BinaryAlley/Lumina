#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Requests.Plugins;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Plugins.UpdatePluginSettings;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Plugins;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Plugins.UpdatePluginSettings;

/// <summary>
/// Contains unit tests for the <see cref="UpdatePluginSettingsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdatePluginSettingsEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly UpdatePluginSettingsEndpoint _sut;
    private readonly UpdatePluginSettingsRequestFixture _updatePluginSettingsRequestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePluginSettingsEndpointTests"/> class.
    /// </summary>
    public UpdatePluginSettingsEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<UpdatePluginSettingsEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldUpdatePluginSettingsViaApiAndReturnSuccess()
    {
        // Arrange
        UpdatePluginSettingsRequest request = _updatePluginSettingsRequestFixture.Create();
        _mockApiHttpClient.PutAsync<Lumina.Presentation.Web.Common.Requests.Common.EmptyRequest, UpdatePluginSettingsRequest>(Arg.Any<string>(), Arg.Any<UpdatePluginSettingsRequest>(), Arg.Any<CancellationToken>())
            .Returns(new Lumina.Presentation.Web.Common.Requests.Common.EmptyRequest());

        // Act
        IResult result = await _sut.ExecuteAsync(request, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        string expectedEndpoint = ApiRoutes.Plugins.UPDATE_PLUGIN_SETTINGS.Replace("{pluginId}", request.PluginId.ToString());
        await _mockApiHttpClient.Received(1).PutAsync<Lumina.Presentation.Web.Common.Requests.Common.EmptyRequest, UpdatePluginSettingsRequest>(
            expectedEndpoint,
            Arg.Is<UpdatePluginSettingsRequest>(settings => settings.PluginId == request.PluginId),
            Arg.Any<CancellationToken>());
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
    }
}
