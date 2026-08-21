#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Themes;
using Lumina.Application.Common.Mapping.Themes;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Themes;
using Lumina.Contracts.Responses.Themes;
using Lumina.Domain.SharedKernel.Common.Enums.Themes;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Themes;

/// <summary>
/// Contains unit tests for the <see cref="ThemeEntityMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThemeEntityMappingTests
{
    private readonly ThemeEntityFixture _themeEntityFixture = new();

    [Fact]
    public void ToResponse_WhenMappingValidThemeEntity_ShouldMapAllFields()
    {
        // Arrange
        ThemeEntity theme = _themeEntityFixture.Create(
            themeId: "my-theme",
            name: "My Theme",
            description: "A nice theme",
            author: "Jane Doe",
            version: "1.2.3",
            previewPath: "preview.png",
            installSource: ThemeInstallSource.Bundled,
            isCurrent: true,
            isDeleted: false);

        // Act
        ThemeResponse result = theme.ToResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(theme.Id, result.Id);
        Assert.Equal(theme.ThemeId, result.ThemeId);
        Assert.Equal(theme.Name, result.Name);
        Assert.Equal(theme.Description, result.Description);
        Assert.Equal(theme.Author, result.Author);
        Assert.Equal(theme.Version, result.Version);
        Assert.Equal(theme.PreviewPath, result.PreviewPath);
        Assert.Equal(theme.InstallSource, result.InstallSource);
        Assert.Equal(theme.IsCurrent, result.IsCurrent);
        Assert.Equal(theme.InstalledAtUtc, result.InstalledAtUtc);
    }

    [Fact]
    public void ToResponse_WhenPreviewPathIsNull_ShouldMapNullPreviewPath()
    {
        // Arrange
        ThemeEntity theme = _themeEntityFixture.Create();
        theme.PreviewPath = null;

        // Act
        ThemeResponse result = theme.ToResponse();

        // Assert
        Assert.Null(result.PreviewPath);
    }

    [Fact]
    public void ToResponse_WhenIsCurrentIsNull_ShouldMapNullIsCurrent()
    {
        // Arrange
        ThemeEntity theme = _themeEntityFixture.Create();
        theme.IsCurrent = null;

        // Act
        ThemeResponse result = theme.ToResponse();

        // Assert
        Assert.Null(result.IsCurrent);
    }

    [Fact]
    public void ToResponse_WhenMappingUploadedTheme_ShouldMapInstallSourceAsUploaded()
    {
        // Arrange
        ThemeEntity theme = _themeEntityFixture.Create(installSource: ThemeInstallSource.Uploaded, isCurrent: null);

        // Act
        ThemeResponse result = theme.ToResponse();

        // Assert
        Assert.Equal(ThemeInstallSource.Uploaded, result.InstallSource);
    }
}
