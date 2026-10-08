#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;

/// <summary>
/// Fixture class for the <see cref="AlbumCatalogNumberEntity"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class AlbumCatalogNumberEntityFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="AlbumCatalogNumberEntity"/>.
    /// </summary>
    /// <param name="catalogNumber">Optional. The catalog number assigned by the label.</param>
    /// <returns>The created <see cref="AlbumCatalogNumberEntity"/>.</returns>
    public AlbumCatalogNumberEntity Create(
        string? catalogNumber = null)
    {
        return new AlbumCatalogNumberEntity(catalogNumber ?? _faker.Random.String2(_faker.Random.Number(1, 50)));
    }

    /// <summary>
    /// Creates a list of <see cref="AlbumCatalogNumberEntity"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<AlbumCatalogNumberEntity> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
