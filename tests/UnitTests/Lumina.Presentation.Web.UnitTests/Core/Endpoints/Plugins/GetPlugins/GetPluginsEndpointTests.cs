#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Plugins;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Plugins.GetPlugins;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Plugins.GetPlugins;

/// <summary>
/// Contains unit tests for the <see cref="GetPluginsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPluginsEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly GetPluginsEndpoint _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPluginsEndpointTests"/> class.
    /// </summary>
    public GetPluginsEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<GetPluginsEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiReturnsPlugins_ShouldReturnSuccessJsonWithPlugins()
    {
        // Arrange
        PluginDto[] expectedPlugins = [new PluginDto { Id = Guid.NewGuid(), Name = "OpenLibrary", Author = "Test Author", Version = "1.0.0" }];
        _mockApiHttpClient.GetAsync<PluginDto[]>(ApiRoutes.Plugins.GET_PLUGINS, Arg.Any<CancellationToken>())
            .Returns(expectedPlugins);

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        PluginDto[]? returnedPlugins = jsonDocument.RootElement.GetProperty("data").Deserialize<PluginDto[]>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.Equal(expectedPlugins.Select(plugin => plugin.Name), returnedPlugins!.Select(plugin => plugin.Name));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldRequestPluginsFromApi()
    {
        // Arrange
        _mockApiHttpClient.GetAsync<PluginDto[]>(ApiRoutes.Plugins.GET_PLUGINS, Arg.Any<CancellationToken>())
            .Returns([new PluginDto { Id = Guid.NewGuid(), Name = "OpenLibrary" }]);

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<PluginDto[]>(ApiRoutes.Plugins.GET_PLUGINS, Arg.Any<CancellationToken>());
    }
}
