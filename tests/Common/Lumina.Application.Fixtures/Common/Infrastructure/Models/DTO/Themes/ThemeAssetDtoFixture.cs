#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.Infrastructure.Models.DTO.Themes;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.Infrastructure.Models.DTO.Themes;

/// <summary>
/// Fixture class for the <see cref="ThemeAssetDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThemeAssetDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="ThemeAssetDto"/>.
    /// </summary>
    /// <param name="bytes">Optional. The bytes of the asset file.</param>
    /// <param name="contentType">Optional. The MIME content type of the asset file.</param>
    /// <returns>The created <see cref="ThemeAssetDto"/>.</returns>
    public ThemeAssetDto Create(
        byte[]? bytes = null, 
        string? contentType = null)
    {
        return new ThemeAssetDto(
            bytes ?? _faker.Random.Bytes(10),
            contentType ?? _faker.System.MimeType());
    }

    /// <summary>
    /// Creates a list of <see cref="ThemeAssetDto"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<ThemeAssetDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
