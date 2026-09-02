#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Models.DTO.Themes;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Lumina.Infrastructure.Common.Models.DTO.Configuration;
using Lumina.Infrastructure.Core.Themes;
using Lumina.Infrastructure.Fixtures.Common.Models.DTO.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.Themes;

/// <summary>
/// Contains unit tests for the <see cref="ThemeService"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThemeServiceTests
{
    private readonly string _storageRootPath;
    private readonly ThemeEngineOptionsDto _options;
    private readonly ThemeEngineOptionsDtoFixture _themeEngineOptionsDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeServiceTests"/> class.
    /// </summary>
    public ThemeServiceTests()
    {
        _storageRootPath = Path.Combine(Path.GetTempPath(), $"lumina-themes-{Guid.NewGuid():N}");
        _options = _themeEngineOptionsDtoFixture.Create(
            storagePath: Path.Combine(_storageRootPath, "themes"),
            bundledThemesPath: Path.Combine(_storageRootPath, "bundled"),
            defaultThemeId: "lumina-default",
            maxArchiveBytes: 8 * 1024 * 1024,
            maxExpandedBytes: 24 * 1024 * 1024,
            maxSingleFileBytes: 6 * 1024 * 1024,
            maxEntries: 250);
    }

    [Fact]
    public void MaxArchiveBytes_WhenConfigured_ShouldReturnConfiguredValue()
    {
        // Arrange
        ThemeEngineOptionsDto options = _themeEngineOptionsDtoFixture.Create(
            storagePath: Path.Combine(_storageRootPath, "themes"),
            bundledThemesPath: Path.Combine(_storageRootPath, "bundled"),
            defaultThemeId: "lumina-default",
            maxArchiveBytes: 8 * 1024 * 1024,
            maxExpandedBytes: 24 * 1024 * 1024,
            maxSingleFileBytes: 6 * 1024 * 1024,
            maxEntries: 250);
        options.MaxArchiveBytes = 12345;
        ThemeService sut = CreateSut(options);

        // Act
        long result = sut.MaxArchiveBytes;

        // Assert
        Assert.Equal(12345, result);
    }

    [Fact]
    public void DefaultThemeId_WhenConfigured_ShouldReturnConfiguredValue()
    {
        // Arrange
        ThemeEngineOptionsDto options = _themeEngineOptionsDtoFixture.Create(
            storagePath: Path.Combine(_storageRootPath, "themes"),
            bundledThemesPath: Path.Combine(_storageRootPath, "bundled"),
            defaultThemeId: "lumina-default",
            maxArchiveBytes: 8 * 1024 * 1024,
            maxExpandedBytes: 24 * 1024 * 1024,
            maxSingleFileBytes: 6 * 1024 * 1024,
            maxEntries: 250);
        options.DefaultThemeId = "custom-default";
        ThemeService sut = CreateSut(options);

        // Act
        string result = sut.DefaultThemeId;

        // Assert
        Assert.Equal("custom-default", result);
    }

    [Fact]
    public async Task GetAssetAsync_WhenPathSegmentIsRenamedByPathService_ShouldReturnPackageInvalidError()
    {
        // Arrange
        IPathService pathService = Substitute.For<IPathService>();
        pathService.SanitizeSegment(Arg.Any<string>())
            .Returns(callInfo =>
            {
                string segment = callInfo.Arg<string>();
                string sanitized = segment == "assets" ? "renamed-assets" : segment;
                return PathSegment.Create(sanitized, isDirectory: true, isDrive: false);
            });
        ThemeService sut = new(Options.Create(_options), Substitute.For<ILogger<ThemeService>>(), pathService);

        // Act
        Result<ThemeAssetDto> result = await sut.GetAssetAsync("test-theme", "assets/logo.png", CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Theme.Package.Invalid", result.FirstError.Code);
    }

    /// <summary>
    /// Creates a <see cref="ThemeService"/> instance using the provided options.
    /// </summary>
    /// <param name="options">The theme engine options to configure the service with.</param>
    /// <returns>The created service.</returns>
    private static ThemeService CreateSut(ThemeEngineOptionsDto options)
    {
        ILogger<ThemeService> logger = Substitute.For<ILogger<ThemeService>>();
        IPathService pathService = Substitute.For<IPathService>();
        pathService.SanitizeSegment(Arg.Any<string>())
            .Returns(callInfo => PathSegment.Create(callInfo.Arg<string>(), isDirectory: true, isDrive: false));
        return new ThemeService(Options.Create(options), logger, pathService);
    }
}
