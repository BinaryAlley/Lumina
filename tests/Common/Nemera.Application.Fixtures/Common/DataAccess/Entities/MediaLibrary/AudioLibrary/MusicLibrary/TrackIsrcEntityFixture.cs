#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;

/// <summary>
/// Fixture class for the <see cref="TrackIsrcEntity"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class TrackIsrcEntityFixture
{
    private const string LETTERS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string ALPHANUMERICS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private const string DIGITS = "0123456789";

    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="TrackIsrcEntity"/>.
    /// </summary>
    /// <param name="value">Optional. The value of the ISRC.</param>
    /// <returns>The created <see cref="TrackIsrcEntity"/>.</returns>
    public TrackIsrcEntity Create(string? value = null)
    {
        // ISRC codes follow the domain format: two letters, three alphanumeric characters and seven digits.
        return new TrackIsrcEntity(value ?? (
            _faker.Random.String2(2, LETTERS)
            + _faker.Random.String2(3, ALPHANUMERICS)
            + _faker.Random.String2(7, DIGITS)));
    }

    /// <summary>
    /// Creates a list of <see cref="TrackIsrcEntity"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<TrackIsrcEntity> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
