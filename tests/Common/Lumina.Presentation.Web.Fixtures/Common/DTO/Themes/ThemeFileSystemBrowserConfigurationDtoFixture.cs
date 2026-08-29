#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.Themes;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.Themes;

/// <summary>
/// Fixture class for generating <see cref="ThemeFileSystemBrowserConfigurationDto"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThemeFileSystemBrowserConfigurationDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="ThemeFileSystemBrowserConfigurationDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="serverBasePath">Optional base URL of the remote API.</param>
    /// <param name="clientBasePath">Optional base URL of the Web application.</param>
    /// <param name="path">Optional initial path displayed by the file system browser.</param>
    /// <param name="viewMode">Optional initial view mode of the file system browser.</param>
    /// <param name="iconSize">Optional initial icon size of the file system browser.</param>
    /// <returns>A configured <see cref="ThemeFileSystemBrowserConfigurationDto"/> instance.</returns>
    public ThemeFileSystemBrowserConfigurationDto Create(
        string? serverBasePath = null,
        string? clientBasePath = null,
        string? path = null,
        string? viewMode = null,
        string? iconSize = null)
    {
        return new ThemeFileSystemBrowserConfigurationDto(
            ServerBasePath: serverBasePath ?? $"http://localhost:{_faker.Random.UShort()}/api/v1/",
            ClientBasePath: clientBasePath ?? $"http://localhost:{_faker.Random.UShort()}/",
            Path: path ?? (_faker.Random.Bool() ? "/" : "C:\\Users\\"),
            ViewMode: viewMode ?? _faker.Random.Word(),
            IconSize: iconSize ?? _faker.Random.Word());
    }

    /// <summary>
    /// Creates multiple <see cref="ThemeFileSystemBrowserConfigurationDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="ThemeFileSystemBrowserConfigurationDto"/> instances.</returns>
    public List<ThemeFileSystemBrowserConfigurationDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
