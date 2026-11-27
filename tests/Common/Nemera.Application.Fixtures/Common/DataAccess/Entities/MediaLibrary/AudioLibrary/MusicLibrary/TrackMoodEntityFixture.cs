#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;

/// <summary>
/// Fixture class for the <see cref="TrackMoodEntity"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class TrackMoodEntityFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="TrackMoodEntity"/>.
    /// </summary>
    /// <param name="name">Optional. The name of the mood.</param>
    /// <returns>The created <see cref="TrackMoodEntity"/>.</returns>
    public TrackMoodEntity Create(
        string? name = null)
    {
        return new TrackMoodEntity(name ?? _faker.Lorem.Word());
    }

    /// <summary>
    /// Creates a list of <see cref="TrackMoodEntity"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<TrackMoodEntity> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
