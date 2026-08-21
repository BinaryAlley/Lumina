#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.Themes;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.Themes;

/// <summary>
/// Fixture class for generating <see cref="ThemeInfoDto"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThemeInfoDtoFixture
{
    /// <summary>
    /// Creates a new <see cref="ThemeInfoDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="id">Optional manifest id of the theme.</param>
    /// <param name="name">Optional display name of the theme.</param>
    /// <param name="description">Optional description of the theme.</param>
    /// <param name="author">Optional author of the theme.</param>
    /// <param name="version">Optional semantic version of the theme.</param>
    /// <param name="previewUrl">Optional URL of the theme preview image.</param>
    /// <param name="isBundled">Whether the theme ships with the application.</param>
    /// <param name="isDeleted">Whether the theme was deleted by the user.</param>
    /// <returns>A configured <see cref="ThemeInfoDto"/> instance.</returns>
    public ThemeInfoDto Create(
        string? id = null,
        string? name = null,
        string? description = null,
        string? author = null,
        string? version = null,
        string? previewUrl = null,
        bool? isBundled = null,
        bool? isDeleted = null)
    {
        Faker faker = new();
        return new ThemeInfoDto(
            Id: id ?? faker.Lorem.Word(),
            Name: name ?? faker.Commerce.ProductName(),
            Description: description ?? faker.Lorem.Sentence(),
            Author: author ?? faker.Name.FullName(),
            Version: version ?? faker.System.Semver(),
            PreviewUrl: previewUrl ?? $"/theme-assets/{faker.Lorem.Word()}/preview.png",
            IsBundled: isBundled ?? faker.Random.Bool(),
            IsDeleted: isDeleted ?? faker.Random.Bool()
        );
    }

    /// <summary>
    /// Creates multiple <see cref="ThemeInfoDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="ThemeInfoDto"/> instances.</returns>
    public List<ThemeInfoDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
