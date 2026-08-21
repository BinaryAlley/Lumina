#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.Infrastructure.Models.DTO.Themes;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.Infrastructure.Models.DTO.Themes;

/// <summary>
/// Fixture class for the <see cref="ThemeManifestDto"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThemeManifestDtoFixture
{
    /// <summary>
    /// Creates a random valid <see cref="ThemeManifestDto"/>.
    /// </summary>
    /// <param name="schemaVersion">Optional schema version of the manifest.</param>
    /// <param name="id">Optional unique identifier of the theme.</param>
    /// <param name="name">Optional display name of the theme.</param>
    /// <param name="description">Optional description of the theme.</param>
    /// <param name="author">Optional author of the theme.</param>
    /// <param name="version">Optional version of the theme.</param>
    /// <param name="preview">Optional preview image path of the theme, or <see langword="null"/> when the theme has no preview.</param>
    /// <param name="includePreview">Whether to set <paramref name="preview"/>, or <see langword="null"/> when <see langword="false"/>.</param>
    /// <param name="templates">Optional template mappings of the theme.</param>
    /// <returns>A configured <see cref="ThemeManifestDto"/> instance.</returns>
    public ThemeManifestDto Create(
        int? schemaVersion = null,
        string? id = null,
        string? name = null,
        string? description = null,
        string? author = null,
        string? version = null,
        string? preview = null,
        bool includePreview = false,
        Dictionary<string, string>? templates = null)
    {
        return new Faker<ThemeManifestDto>()
            .CustomInstantiator(f => new ThemeManifestDto
            {
                SchemaVersion = default,
                Id = default!,
                Name = default!,
                Description = default!,
                Author = default!,
                Version = default!,
                Preview = default,
                Templates = default!
            })
            .RuleFor(manifest => manifest.SchemaVersion, f => schemaVersion ?? f.Random.Int(1, 5))
            .RuleFor(manifest => manifest.Id, f => id ?? f.Lorem.Slug(2))
            .RuleFor(manifest => manifest.Name, f => name ?? f.Commerce.ProductName())
            .RuleFor(manifest => manifest.Description, f => description ?? f.Lorem.Sentence())
            .RuleFor(manifest => manifest.Author, f => author ?? f.Name.FullName())
            .RuleFor(manifest => manifest.Version, f => version ?? f.System.Semver())
            .RuleFor(manifest => manifest.Preview, f => includePreview ? (preview ?? (f.Random.Bool() ? f.System.FilePath() : null)) : null)
            .RuleFor(manifest => manifest.Templates, f => templates ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["home"] = "templates/home.html" })
            .Generate();
    }

    /// <summary>
    /// Creates a list of <see cref="ThemeManifestDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="ThemeManifestDto"/> instances.</returns>
    public List<ThemeManifestDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
