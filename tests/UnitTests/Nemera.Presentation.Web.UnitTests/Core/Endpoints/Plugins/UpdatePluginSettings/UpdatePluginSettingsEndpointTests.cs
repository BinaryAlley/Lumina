#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Nemera.Presentation.Web.Common.Api;
using Nemera.Presentation.Web.Common.Requests.Plugins;
using Nemera.Presentation.Web.Common.Routes;
using Nemera.Presentation.Web.Core.Endpoints.Plugins.UpdatePluginSettings;
using Nemera.Presentation.Web.Fixtures.Common.Requests.Plugins;
using Nemera.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Web.UnitTests.Core.Endpoints.Plugins.UpdatePluginSettings;

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
        _mockApiHttpClient.PutIgnoringResponseAsync<UpdatePluginSettingsRequest>(Arg.Any<string>(), Arg.Any<UpdatePluginSettingsRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        IResult result = await _sut.ExecuteAsync(request, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        string expectedEndpoint = ApiRoutes.Plugins.UPDATE_PLUGIN_SETTINGS.Replace("{pluginId}", request.PluginId.ToString());
        await _mockApiHttpClient.Received(1).PutIgnoringResponseAsync<UpdatePluginSettingsRequest>(
            expectedEndpoint,
            Arg.Is<UpdatePluginSettingsRequest>(settings => settings.PluginId == request.PluginId),
            Arg.Any<CancellationToken>());
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
    }
}
