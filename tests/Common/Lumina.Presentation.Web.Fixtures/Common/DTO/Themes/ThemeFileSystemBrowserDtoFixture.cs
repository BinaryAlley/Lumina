#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.Themes;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.Themes;

/// <summary>
/// Fixture class for generating <see cref="ThemeFileSystemBrowserDto"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThemeFileSystemBrowserDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="ThemeFileSystemBrowserDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="appBase">Optional absolute base URL of the Web application.</param>
    /// <param name="assetBase">Optional base URL of the theme assets.</param>
    /// <param name="iconBaseUrl">Optional base URL of the theme file type icons.</param>
    /// <param name="fileIconsUrl">Optional URL of the theme file icons mapping.</param>
    /// <param name="treeNodeTemplate">Optional raw template source of the tree view node.</param>
    /// <param name="explorerItemTemplate">Optional raw template source of the explorer item.</param>
    /// <param name="pathSegmentTemplate">Optional raw template source of the address bar path segment.</param>
    /// <param name="strings">Optional localized strings of the file system browser.</param>
    /// <returns>A configured <see cref="ThemeFileSystemBrowserDto"/> instance.</returns>
    public ThemeFileSystemBrowserDto Create(
        string? appBase = null,
        string? assetBase = null,
        string? iconBaseUrl = null,
        string? fileIconsUrl = null,
        string? treeNodeTemplate = null,
        string? explorerItemTemplate = null,
        string? pathSegmentTemplate = null,
        IReadOnlyDictionary<string, object?>? strings = null)
    {
        string themeId = _faker.Lorem.Word();
        return new ThemeFileSystemBrowserDto(
            AppBase: appBase ?? $"https://localhost:{_faker.Random.UShort()}/",
            AssetBase: assetBase ?? $"/theme-assets/{themeId}/assets",
            IconBaseUrl: iconBaseUrl ?? $"/theme-assets/{themeId}/assets/images/icons",
            FileIconsUrl: fileIconsUrl ?? $"/theme-assets/{themeId}/assets/file-icons.json",
            TreeNodeTemplate: treeNodeTemplate ?? "<div class=\"tree-node\" data-path=\"{{path}}\">{{name}}</div>",
            ExplorerItemTemplate: explorerItemTemplate ?? "<div class=\"e {{cssClass}}\" data-path=\"{{path}}\">{{name}}</div>",
            PathSegmentTemplate: pathSegmentTemplate ?? "<li id=\"{{id}}\">{{path}}</li>",
            Strings: strings ?? new Dictionary<string, object?> { ["cancel"] = "Cancel" });
    }

    /// <summary>
    /// Creates multiple <see cref="ThemeFileSystemBrowserDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="ThemeFileSystemBrowserDto"/> instances.</returns>
    public List<ThemeFileSystemBrowserDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
