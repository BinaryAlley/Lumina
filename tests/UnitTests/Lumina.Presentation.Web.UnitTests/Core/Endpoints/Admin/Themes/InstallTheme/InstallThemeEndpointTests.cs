#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Themes;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Admin.Themes.InstallTheme;
using Lumina.Presentation.Web.Core.Themes;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Themes;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Primitives;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Admin.Themes.InstallTheme;

/// <summary>
/// Contains unit tests for the <see cref="InstallThemeEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class InstallThemeEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly InstallThemeEndpoint _sut;
    private readonly ThemeResponseDtoFixture _themeResponseDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="InstallThemeEndpointTests"/> class.
    /// </summary>
    public InstallThemeEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        ThemeService themeService = new(_mockApiHttpClient);
        _sut = Factory.Create<InstallThemeEndpoint>(themeService);
    }

    [Fact]
    public async Task ExecuteAsync_WhenArchiveUploaded_ShouldInstallThemeAndReturnSuccessJson()
    {
        // Arrange
        ThemeResponseDto installedTheme = _themeResponseDtoFixture.Create(themeId: "installed-theme");
        _mockApiHttpClient.PostMultipartAsync<ThemeResponseDto>(ApiRoutes.Themes.INSTALL_THEME, Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(installedTheme);
        ConfigureFormWithArchive([1, 2, 3, 4], "theme-pack.zip");

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using (JsonDocument jsonDocument = JsonDocument.Parse(body))
        {
            Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
            Assert.Equal("installed-theme", jsonDocument.RootElement.GetProperty("data").GetProperty("id").GetString());
        }

        await _mockApiHttpClient.Received(1).PostMultipartAsync<ThemeResponseDto>(
            ApiRoutes.Themes.INSTALL_THEME,
            Arg.Any<Stream>(),
            "theme-pack.zip",
            "archive",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoArchiveUploaded_ShouldReturnProblemWithBadRequest()
    {
        // Arrange
        IFormFeature formFeature = Substitute.For<IFormFeature>();
        formFeature.Form.Returns(new FormCollection([]));
        _sut.HttpContext.Features.Set(formFeature);

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, problemResult.StatusCode);
        await _mockApiHttpClient.DidNotReceive().PostMultipartAsync<ThemeResponseDto>(Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    private void ConfigureFormWithArchive(byte[] content, string fileName)
    {
        // the stream stays open for the duration of the test, since the endpoint reads it through the form file during execution
        MemoryStream archiveStream = new(content);
        IFormFile formFile = new FormFile(archiveStream, 0, content.Length, "archive", fileName);
        FormFileCollection files = [formFile];
        IFormCollection form = new FormCollection([], files);
        _sut.HttpContext.Request.ContentType = "multipart/form-data; boundary=----test";
        // a real FormFeature is used instead of a substitute, because the endpoint execution reads the form through the
        // request machinery, which would bypass a mocked feature and parse the (empty) request body
        _sut.HttpContext.Features.Set<IFormFeature>(new FormFeature(form));
    }
}
