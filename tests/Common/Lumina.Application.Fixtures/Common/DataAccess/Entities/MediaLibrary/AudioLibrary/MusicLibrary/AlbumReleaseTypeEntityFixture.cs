#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;

/// <summary>
/// Fixture class for the <see cref="AlbumReleaseTypeEntity"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class AlbumReleaseTypeEntityFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="AlbumReleaseTypeEntity"/>.
    /// </summary>
    /// <param name="releaseType">Optional. The type of the release.</param>
    /// <returns>The created <see cref="AlbumReleaseTypeEntity"/>.</returns>
    public AlbumReleaseTypeEntity Create(
        MusicReleaseType? releaseType = null)
    {
        return new AlbumReleaseTypeEntity(releaseType ?? _faker.PickRandom<MusicReleaseType>());
    }

    /// <summary>
    /// Creates a list of <see cref="AlbumReleaseTypeEntity"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<AlbumReleaseTypeEntity> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
