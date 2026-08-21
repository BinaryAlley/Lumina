#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.Themes;
using Lumina.Domain.SharedKernel.Common.Enums.Themes;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DataAccess.Entities.Themes;

/// <summary>
/// Fixture class for the <see cref="ThemeEntity"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThemeEntityFixture
{
    /// <summary>
    /// Creates a random valid <see cref="ThemeEntity"/>.
    /// </summary>
    /// <param name="id">Optional database Id of the theme.</param>
    /// <param name="themeId">Optional manifest id of the theme.</param>
    /// <param name="name">Optional display name of the theme.</param>
    /// <param name="description">Optional description of the theme.</param>
    /// <param name="author">Optional author of the theme.</param>
    /// <param name="version">Optional version of the theme.</param>
    /// <param name="previewPath">Optional preview image path of the theme, or <see langword="null"/> when the theme has no preview.</param>
    /// <param name="includePreviewPath">Whether to set <paramref name="previewPath"/>, or <see langword="null"/> when <see langword="false"/>.</param>
    /// <param name="installSource">Optional source the theme was installed from.</param>
    /// <param name="isCurrent">Optional value indicating whether the theme is the currently active one.</param>
    /// <param name="includeIsCurrent">Whether to set <paramref name="isCurrent"/>, or <see langword="null"/> when <see langword="false"/>.</param>
    /// <param name="isDeleted">Optional value indicating whether the theme was soft deleted.</param>
    /// <param name="installedAtUtc">Optional UTC timestamp when the theme was installed.</param>
    /// <param name="createdOnUtc">Optional UTC timestamp when the entity was created.</param>
    /// <param name="createdBy">Optional Id of the user that created the entity.</param>
    /// <param name="updatedOnUtc">Optional UTC timestamp when the entity was updated.</param>
    /// <param name="includeUpdatedOnUtc">Whether to set <paramref name="updatedOnUtc"/>, or <see langword="null"/> when <see langword="false"/>.</param>
    /// <param name="updatedBy">Optional Id of the user that updated the entity.</param>
    /// <param name="includeUpdatedBy">Whether to set <paramref name="updatedBy"/>, or <see langword="null"/> when <see langword="false"/>.</param>
    /// <returns>A configured <see cref="ThemeEntity"/> instance.</returns>
    public ThemeEntity Create(
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
        bool? isDeleted = null,
        DateTime? installedAtUtc = null,
        DateTime? createdOnUtc = null,
        Guid? createdBy = null,
        DateTime? updatedOnUtc = null,
        bool includeUpdatedOnUtc = false,
        Guid? updatedBy = null,
        bool includeUpdatedBy = false)
    {
        Guid resolvedCreatedBy = createdBy ?? Guid.NewGuid();
        return new Faker<ThemeEntity>()
            .CustomInstantiator(f => new ThemeEntity
            {
                Id = default,
                ThemeId = default!,
                Name = default!,
                Description = default!,
                Author = default!,
                Version = default!,
                PreviewPath = default,
                InstallSource = default,
                IsCurrent = default,
                IsDeleted = default,
                InstalledAtUtc = default,
                CreatedOnUtc = default,
                CreatedBy = default,
                UpdatedOnUtc = default,
                UpdatedBy = default
            })
            .RuleFor(theme => theme.Id, f => id ?? f.Random.Guid())
            .RuleFor(theme => theme.ThemeId, f => themeId ?? f.Lorem.Slug(2))
            .RuleFor(theme => theme.Name, f => name ?? f.Commerce.ProductName())
            .RuleFor(theme => theme.Description, f => description ?? f.Lorem.Sentence())
            .RuleFor(theme => theme.Author, f => author ?? f.Name.FullName())
            .RuleFor(theme => theme.Version, f => version ?? f.System.Semver())
            .RuleFor(theme => theme.PreviewPath, f => includePreviewPath ? (previewPath ?? f.System.FilePath()) : null)
            .RuleFor(theme => theme.InstallSource, f => installSource ?? f.PickRandom<ThemeInstallSource>())
            .RuleFor(theme => theme.IsCurrent, f => includeIsCurrent ? (isCurrent ?? (f.Random.Bool() ? true : null)) : null)
            .RuleFor(theme => theme.IsDeleted, f => isDeleted ?? f.Random.Bool())
            .RuleFor(theme => theme.InstalledAtUtc, f => installedAtUtc ?? f.Date.Recent().ToUniversalTime())
            .RuleFor(theme => theme.CreatedOnUtc, f => createdOnUtc ?? f.Date.Past().ToUniversalTime())
            .RuleFor(theme => theme.CreatedBy, resolvedCreatedBy)
            .RuleFor(theme => theme.UpdatedOnUtc, f => includeUpdatedOnUtc ? (updatedOnUtc ?? f.Date.Recent().ToUniversalTime()) : null)
            .RuleFor(theme => theme.UpdatedBy, f => includeUpdatedBy ? (updatedBy ?? f.Random.Guid()) : null)
            .Generate();
    }

    /// <summary>
    /// Creates a list of <see cref="ThemeEntity"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="ThemeEntity"/> instances.</returns>
    public List<ThemeEntity> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
