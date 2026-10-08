#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;

/// <summary>
/// Fixture class for the <see cref="ArtistIpiEntity"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ArtistIpiEntityFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="ArtistIpiEntity"/>.
    /// </summary>
    /// <param name="value">Optional. The value of the IPI code.</param>
    /// <returns>The created <see cref="ArtistIpiEntity"/>.</returns>
    public ArtistIpiEntity Create(
        string? value = null)
    {
        return new ArtistIpiEntity(value ?? _faker.Random.String2(_faker.Random.Number(1, 20), "0123456789"));
    }

    /// <summary>
    /// Creates a list of <see cref="ArtistIpiEntity"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<ArtistIpiEntity> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
