#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.Themes;
using Lumina.Presentation.Web.Common.Enums.Themes;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.Themes;

/// <summary>
/// Fixture class for generating <see cref="ThemeResponseDto"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThemeResponseDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="ThemeResponseDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="id">Optional database identifier of the theme.</param>
    /// <param name="themeId">Optional manifest id of the theme.</param>
    /// <param name="name">Optional display name of the theme.</param>
    /// <param name="description">Optional description of the theme.</param>
    /// <param name="author">Optional author of the theme.</param>
    /// <param name="version">Optional semantic version of the theme.</param>
    /// <param name="previewPath">Optional preview image path of the theme, or <see langword="null"/> when the theme has no preview.</param>
    /// <param name="includePreviewPath">Whether to set <paramref name="previewPath"/>, or <see langword="null"/> when <see langword="false"/>.</param>
    /// <param name="installSource">Optional source the theme was installed from.</param>
    /// <param name="isCurrent">Optional value indicating whether the theme is the active one.</param>
    /// <param name="includeIsCurrent">Whether to set <paramref name="isCurrent"/>, or <see langword="null"/> when <see langword="false"/>.</param>
    /// <param name="installedAtUtc">Optional UTC timestamp at which the theme was installed.</param>
    /// <param name="isDeleted">Optional value indicating whether the theme was deleted by the user.</param>
    /// <returns>A configured <see cref="ThemeResponseDto"/> instance.</returns>
    public ThemeResponseDto Create(
        Guid? id = null,
        string? themeId = null,
        string? name = null,
        string? description = null,
        string? author = null,
        string? version = null,
        string? previewPath = null,
        bool includePreviewPath = false,
        ThemeInstallSource? installSource = null,
        bool? isCurrent = null,
        bool includeIsCurrent = false,
        DateTime? installedAtUtc = null,
        bool? isDeleted = null)
    {
        return new ThemeResponseDto(
            Id: id ?? Guid.NewGuid(),
            ThemeId: themeId ?? _faker.Lorem.Word(),
            Name: name ?? _faker.Commerce.ProductName(),
            Description: description ?? _faker.Lorem.Sentence(),
            Author: author ?? _faker.Name.FullName(),
            Version: version ?? _faker.System.Semver(),
            PreviewPath: includePreviewPath ? (previewPath ?? (_faker.Random.Bool() ? _faker.System.FilePath() : null)) : null,
            InstallSource: installSource ?? ThemeInstallSource.Uploaded,
            IsCurrent: includeIsCurrent ? (isCurrent ?? (_faker.Random.Bool() ? true : null)) : null,
            InstalledAtUtc: installedAtUtc ?? _faker.Date.Recent(),
            IsDeleted: isDeleted ?? _faker.Random.Bool()
        );
    }

    /// <summary>
    /// Creates multiple <see cref="ThemeResponseDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="ThemeResponseDto"/> instances.</returns>
    public List<ThemeResponseDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
