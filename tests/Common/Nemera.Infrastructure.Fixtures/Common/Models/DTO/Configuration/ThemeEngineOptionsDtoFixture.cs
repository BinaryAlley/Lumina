#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Infrastructure.Common.Models.DTO.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Infrastructure.Fixtures.Common.Models.DTO.Configuration;

/// <summary>
/// Fixture class for the <see cref="ThemeEngineOptionsDto"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThemeEngineOptionsDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="ThemeEngineOptionsDto"/>.
    /// </summary>
    /// <param name="storagePath">Optional directory where installed theme packs are stored.</param>
    /// <param name="bundledThemesPath">Optional directory where the shipped theme pack archives are located.</param>
    /// <param name="defaultThemeId">Optional identifier of the default theme.</param>
    /// <param name="maxArchiveBytes">Optional maximum size of an uploaded theme archive, in bytes.</param>
    /// <param name="maxExpandedBytes">Optional maximum total size of an extracted theme pack, in bytes.</param>
    /// <param name="maxSingleFileBytes">Optional maximum size of a single file within a theme pack, in bytes.</param>
    /// <param name="maxEntries">Optional maximum number of entries in a theme pack archive.</param>
    /// <returns>A configured <see cref="ThemeEngineOptionsDto"/> instance.</returns>
    public ThemeEngineOptionsDto Create(
        string? storagePath = null,
        string? bundledThemesPath = null,
        string? defaultThemeId = null,
        long? maxArchiveBytes = null,
        long? maxExpandedBytes = null,
        long? maxSingleFileBytes = null,
        int? maxEntries = null)
    {
        return new ThemeEngineOptionsDto
        {
            StoragePath = storagePath ?? _faker.System.DirectoryPath(),
            BundledThemesPath = bundledThemesPath ?? _faker.System.DirectoryPath(),
            DefaultThemeId = defaultThemeId ?? _faker.Lorem.Word(),
            MaxArchiveBytes = maxArchiveBytes ?? _faker.Random.Long(1_000_000, 100_000_000),
            MaxExpandedBytes = maxExpandedBytes ?? _faker.Random.Long(10_000_000, 500_000_000),
            MaxSingleFileBytes = maxSingleFileBytes ?? _faker.Random.Long(1_000_000, 50_000_000),
            MaxEntries = maxEntries ?? _faker.Random.Int(1, 1000)
        };
    }

    /// <summary>
    /// Creates a list of <see cref="ThemeEngineOptionsDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="ThemeEngineOptionsDto"/> instances.</returns>
    public List<ThemeEngineOptionsDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
