#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Plugins;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Plugins.InstallPlugin;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Plugins;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Plugins.InstallPlugin;

/// <summary>
/// Contains unit tests for the <see cref="InstallPluginEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class InstallPluginEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly InstallPluginEndpoint _sut;
    private readonly PluginDtoFixture _pluginDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="InstallPluginEndpointTests"/> class.
    /// </summary>
    public InstallPluginEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<InstallPluginEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenArchiveUploaded_ShouldInstallPluginViaApiAndReturnSuccess()
    {
        // Arrange
        byte[] content = [1, 2, 3, 4];
        string fileName = "plugin.zip";
        _mockApiHttpClient.PostMultipartAsync<PluginDto>(Arg.Any<string>(), Arg.Any<Stream>(), fileName, "archive", Arg.Any<CancellationToken>())
            .Returns(_pluginDtoFixture.Create());
        ConfigureFormWithArchive(content, fileName);

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        await _mockApiHttpClient.Received(1).PostMultipartAsync<PluginDto>(
            ApiRoutes.Plugins.INSTALL_PLUGIN,
            Arg.Any<Stream>(),
            fileName,
            "archive",
            Arg.Any<CancellationToken>());
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
    }

    private void ConfigureFormWithArchive(byte[] content, string fileName)
    {
        // the stream stays open for the duration of the test, since the endpoint reads it through the form file during execution
        MemoryStream archiveStream = new(content);
        IFormFile formFile = new FormFile(archiveStream, 0, content.Length, "archive", fileName);
        FormFileCollection files = [formFile];
        IFormCollection form = new FormCollection([], files);
        _sut.HttpContext.Request.ContentType = "multipart/form-data; boundary=----test";
        _sut.HttpContext.Features.Set<IFormFeature>(new FormFeature(form));
    }
}
