#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Responses.Themes;
using Lumina.Domain.SharedKernel.Common.Enums.Themes;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Responses.Themes;

/// <summary>
/// Fixture class for the <see cref="ThemeResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThemeResponseFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="ThemeResponse"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the theme.</param>
    /// <param name="themeId">Optional. The manifest id of the theme.</param>
    /// <param name="name">Optional. The display name of the theme.</param>
    /// <param name="description">Optional. The description of the theme.</param>
    /// <param name="author">Optional. The author of the theme.</param>
    /// <param name="version">Optional. The version of the theme.</param>
    /// <param name="previewPath">Optional. The preview image path of the theme, or <see langword="null"/> when the theme has no preview.</param>
    /// <param name="includePreviewPath">Whether to set <paramref name="previewPath"/>, or <see langword="null"/> when <see langword="false"/>.</param>
    /// <param name="installSource">Optional. The source the theme was installed from.</param>
    /// <param name="isCurrent">Optional. Whether the theme is the currently active one.</param>
    /// <param name="includeIsCurrent">Whether to set <paramref name="isCurrent"/>, or <see langword="null"/> when <see langword="false"/>.</param>
    /// <param name="installedAtUtc">Optional. The UTC timestamp when the theme was installed.</param>
    /// <returns>The created <see cref="ThemeResponse"/>.</returns>
    public ThemeResponse Create(
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
        DateTime? installedAtUtc = null)
    {
        return new ThemeResponse(
            id ?? Guid.NewGuid(),
            themeId ?? _faker.Lorem.Slug(2),
            name ?? _faker.Commerce.ProductName(),
            description ?? _faker.Lorem.Sentence(),
            author ?? _faker.Name.FullName(),
            version ?? _faker.System.Semver(),
            includePreviewPath ? (previewPath ?? (_faker.Random.Bool() ? _faker.System.FilePath() : null)) : null,
            installSource ?? _faker.PickRandom<ThemeInstallSource>(),
            includeIsCurrent ? (isCurrent ?? (_faker.Random.Bool() ? true : null)) : null,
            installedAtUtc ?? _faker.Date.Recent().ToUniversalTime());
    }

    /// <summary>
    /// Creates a list of <see cref="ThemeResponse"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<ThemeResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
