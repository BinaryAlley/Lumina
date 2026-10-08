#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;

/// <summary>
/// Fixture class for the <see cref="TrackWorkIswcEntity"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class TrackWorkIswcEntityFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="TrackWorkIswcEntity"/>.
    /// </summary>
    /// <param name="value">Optional. The value of the ISWC.</param>
    /// <returns>The created <see cref="TrackWorkIswcEntity"/>.</returns>
    public TrackWorkIswcEntity Create(
        string? value = null)
    {
        return new TrackWorkIswcEntity(value ?? $"T{_faker.Random.AlphaNumeric(10)}");
    }

    /// <summary>
    /// Creates a list of <see cref="TrackWorkIswcEntity"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<TrackWorkIswcEntity> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
