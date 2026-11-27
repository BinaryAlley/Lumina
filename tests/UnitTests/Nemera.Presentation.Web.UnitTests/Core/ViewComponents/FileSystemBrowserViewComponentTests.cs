#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Themes;
using Lumina.Presentation.Web.Common.Primitives;
using Lumina.Presentation.Web.Core.Themes;
using Lumina.Presentation.Web.Core.ViewComponents;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.Extensions.Localization;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.ViewComponents;

/// <summary>
/// Contains unit tests for the <see cref="FileSystemBrowserViewComponent"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class FileSystemBrowserViewComponentTests
{
    private readonly ThemeFileSystemBrowserRenderer _mockRenderer;
    private readonly FileSystemBrowserViewComponent _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileSystemBrowserViewComponentTests"/> class.
    /// </summary>
    public FileSystemBrowserViewComponentTests()
    {
        ThemeService themeService = new(Substitute.For<IApiHttpClient>());
        ThemeFileSystemBrowserBuilder builder = new(Substitute.For<IStringLocalizerFactory>(), Substitute.For<IHttpContextAccessor>());
        _mockRenderer = Substitute.For<ThemeFileSystemBrowserRenderer>(themeService, new ThemeTemplateEngine(), builder);
        _sut = new FileSystemBrowserViewComponent(_mockRenderer);
    }

    [Fact]
    public async Task InvokeAsync_WhenThemedRenderSucceeds_ShouldReturnContentWithRenderedHtml()
    {
        // Arrange
        _mockRenderer.RenderAsync(Arg.Any<ThemeFileSystemBrowserConfigurationDto>(), Arg.Any<CancellationToken>())
            .Returns(Result.From("themed-html"));

        // Act
        IViewComponentResult result = await _sut.InvokeAsync();

        // Assert
        ViewViewComponentResult viewResult = Assert.IsType<ViewViewComponentResult>(result);
        Assert.Equal("Themed", viewResult.ViewName);
        Assert.Equal("themed-html", viewResult.ViewData.Model);
    }

    [Fact]
    public async Task InvokeAsync_WhenThemedRenderFails_ShouldReturnFallbackView()
    {
        // Arrange
        _mockRenderer.RenderAsync(Arg.Any<ThemeFileSystemBrowserConfigurationDto>(), Arg.Any<CancellationToken>())
            .Returns(Error.NotFound("Theme.Template.Unavailable", "The file system browser theme template could not be loaded."));

        // Act
        IViewComponentResult result = await _sut.InvokeAsync();

        // Assert
        ViewViewComponentResult viewResult = Assert.IsType<ViewViewComponentResult>(result);
        Assert.NotNull(viewResult.ViewData);
        Assert.NotNull(viewResult.ViewData!.Model);
        dynamic model = viewResult.ViewData!.Model!;
        string expectedPath = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? @"/" : @"C:\Users\";
        Assert.Equal(expectedPath, (string)model.Path);
        Assert.Equal("list", (string)model.ViewMode);
        Assert.Equal("large", (string)model.IconSize);
    }

    [Fact]
    public async Task InvokeAsync_WhenCalled_ShouldPassConfigurationToRenderer()
    {
        // Arrange
        _mockRenderer.RenderAsync(Arg.Any<ThemeFileSystemBrowserConfigurationDto>(), Arg.Any<CancellationToken>())
            .Returns(Result.From("themed-html"));

        // Act
        await _sut.InvokeAsync();

        // Assert
        await _mockRenderer.Received(1).RenderAsync(
            Arg.Is<ThemeFileSystemBrowserConfigurationDto>(configuration =>
                configuration.ClientBasePath == "http://localhost:5012/" &&
                configuration.ViewMode == "list" &&
                configuration.IconSize == "large"),
            Arg.Any<CancellationToken>());
    }
}
