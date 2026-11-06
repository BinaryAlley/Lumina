#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Plugins;
using Lumina.Presentation.Web.Common.Enums.Plugins;
using Lumina.Presentation.Web.Common.Http;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Plugins.Index;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Plugins;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Plugins.Index;

/// <summary>
/// Contains unit tests for the <see cref="PluginsIndexViewEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PluginsIndexViewEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly PluginsIndexViewEndpoint _sut;
    private readonly PluginDtoFixture _pluginDtoFixture = new();
    private readonly PluginSettingsDtoFixture _pluginSettingsDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginsIndexViewEndpointTests"/> class.
    /// </summary>
    public PluginsIndexViewEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<PluginsIndexViewEndpoint>(_mockApiHttpClient);
        TestHttpContextFactory.ConfigureSession(_sut.HttpContext, TestHttpContextFactory.CreateSession());
    }

    [Fact]
    public async Task ExecuteAsync_WhenALoadedPluginHasNoSettings_ShouldLeaveItOutOfThePluginSettingsViewData()
    {
        // Arrange
        PluginDto plugin = _pluginDtoFixture.Create();
        PluginSettingsDto pluginSettings = _pluginSettingsDtoFixture.Create(pluginId: plugin.Id, includeSchema: false);
        _mockApiHttpClient.GetAsync<PluginDto[]>(ApiRoutes.Plugins.GET_PLUGINS, Arg.Any<CancellationToken>())
            .Returns([plugin]);
        _mockApiHttpClient.GetAsync<PluginSettingsDto>(BuildSettingsEndpoint(plugin.Id), Arg.Any<CancellationToken>())
            .Returns(pluginSettings);

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        Dictionary<Guid, PluginSettingsDto> settingsByPluginId = GetPluginSettingsViewData(result);
        Assert.False(settingsByPluginId.ContainsKey(plugin.Id));
    }

    [Fact]
    public async Task ExecuteAsync_WhenALoadedPluginHasSettings_ShouldExposeItsSettingsInThePluginSettingsViewData()
    {
        // Arrange
        PluginDto plugin = _pluginDtoFixture.Create();
        PluginSettingsDto pluginSettings = _pluginSettingsDtoFixture.Create(pluginId: plugin.Id);
        _mockApiHttpClient.GetAsync<PluginDto[]>(ApiRoutes.Plugins.GET_PLUGINS, Arg.Any<CancellationToken>())
            .Returns([plugin]);
        _mockApiHttpClient.GetAsync<PluginSettingsDto>(BuildSettingsEndpoint(plugin.Id), Arg.Any<CancellationToken>())
            .Returns(pluginSettings);

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        Dictionary<Guid, PluginSettingsDto> settingsByPluginId = GetPluginSettingsViewData(result);
        Assert.True(settingsByPluginId.TryGetValue(plugin.Id, out PluginSettingsDto? actualSettings));
        Assert.Same(pluginSettings, actualSettings);
        await _mockApiHttpClient.Received(1).GetAsync<PluginSettingsDto>(BuildSettingsEndpoint(plugin.Id), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenAPluginFailedToLoad_ShouldNotRequestItsSettings()
    {
        // Arrange
        PluginDto plugin = _pluginDtoFixture.Create(loadStatus: PluginLoadStatus.FailedToLoad);
        _mockApiHttpClient.GetAsync<PluginDto[]>(ApiRoutes.Plugins.GET_PLUGINS, Arg.Any<CancellationToken>())
            .Returns([plugin]);

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.DidNotReceive().GetAsync<PluginSettingsDto>(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldRenderThePluginsViewWithThePlugins()
    {
        // Arrange
        PluginDto[] plugins = [.. _pluginDtoFixture.CreateMany(2)];
        foreach (PluginDto plugin in plugins)
            _mockApiHttpClient.GetAsync<PluginSettingsDto>(BuildSettingsEndpoint(plugin.Id), Arg.Any<CancellationToken>())
                .Returns(_pluginSettingsDtoFixture.Create(pluginId: plugin.Id));
        _mockApiHttpClient.GetAsync<PluginDto[]>(ApiRoutes.Plugins.GET_PLUGINS, Arg.Any<CancellationToken>())
            .Returns(plugins);

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        Assert.Equal("/Core/Views/Admin/Plugins.cshtml", GetRenderedViewName(result));
        Assert.Same(plugins, GetRenderedModel(result));
    }

    /// <summary>
    /// Builds the API route of the settings of the plugin identified by the provided identifier.
    /// </summary>
    /// <param name="pluginId">The unique identifier of the plugin.</param>
    /// <returns>The API route of the plugin settings.</returns>
    private static string BuildSettingsEndpoint(Guid pluginId)
    {
        return ApiRoutes.Plugins.GET_PLUGIN_SETTINGS.Replace("{pluginId}", pluginId.ToString());
    }

    /// <summary>
    /// Reads the plugin settings map exposed to the rendered view.
    /// </summary>
    /// <param name="result">The result returned by the endpoint under test.</param>
    /// <returns>The plugin settings keyed by plugin identifier.</returns>
    private static Dictionary<Guid, PluginSettingsDto> GetPluginSettingsViewData(IResult result)
    {
        IReadOnlyDictionary<string, object?> viewData = GetRenderedViewData(result);
        return Assert.IsType<Dictionary<Guid, PluginSettingsDto>>(viewData["pluginSettings"]);
    }

    /// <summary>
    /// Reads the private view name of the rendered <see cref="RazorViewResult"/>, which the result does not expose publicly.
    /// </summary>
    /// <param name="result">The result returned by the endpoint under test.</param>
    /// <returns>The name or path of the rendered view.</returns>
    private static string GetRenderedViewName(IResult result)
    {
        RazorViewResult viewResult = Assert.IsType<RazorViewResult>(result);
        FieldInfo? viewNameField = typeof(RazorViewResult).GetField("_viewName", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(viewNameField);
        return (string)viewNameField.GetValue(viewResult)!;
    }

    /// <summary>
    /// Reads the private model of the rendered <see cref="RazorViewResult"/>, which the result does not expose publicly.
    /// </summary>
    /// <param name="result">The result returned by the endpoint under test.</param>
    /// <returns>The model passed to the rendered view.</returns>
    private static object? GetRenderedModel(IResult result)
    {
        RazorViewResult viewResult = Assert.IsType<RazorViewResult>(result);
        FieldInfo? modelField = typeof(RazorViewResult).GetField("_model", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(modelField);
        return modelField.GetValue(viewResult);
    }

    /// <summary>
    /// Reads the private view data of the rendered <see cref="RazorViewResult"/>, which the result does not expose publicly.
    /// </summary>
    /// <param name="result">The result returned by the endpoint under test.</param>
    /// <returns>The view data entries passed to the rendered view.</returns>
    private static IReadOnlyDictionary<string, object?> GetRenderedViewData(IResult result)
    {
        RazorViewResult viewResult = Assert.IsType<RazorViewResult>(result);
        FieldInfo? viewDataField = typeof(RazorViewResult).GetField("_viewData", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(viewDataField);
        return (IReadOnlyDictionary<string, object?>)viewDataField.GetValue(viewResult)!;
    }
}
