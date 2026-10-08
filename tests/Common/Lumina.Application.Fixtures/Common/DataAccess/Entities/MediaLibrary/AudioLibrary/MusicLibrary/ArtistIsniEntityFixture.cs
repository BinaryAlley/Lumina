#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;

/// <summary>
/// Fixture class for the <see cref="ArtistIsniEntity"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ArtistIsniEntityFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="ArtistIsniEntity"/>.
    /// </summary>
    /// <param name="value">Optional. The value of the ISNI code.</param>
    /// <returns>The created <see cref="ArtistIsniEntity"/>.</returns>
    public ArtistIsniEntity Create(
        string? value = null)
    {
        return new ArtistIsniEntity(value ?? _faker.Random.String2(_faker.Random.Number(1, 20), "0123456789X"));
    }

    /// <summary>
    /// Creates a list of <see cref="ArtistIsniEntity"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<ArtistIsniEntity> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
