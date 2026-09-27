#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Artists;

/// <summary>
/// Fixture class for the <see cref="ArtistLiteRow"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ArtistLiteRowFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="ArtistLiteRow"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the artist.</param>
    /// <param name="name">Optional. The name of the artist.</param>
    /// <returns>The created <see cref="ArtistLiteRow"/>.</returns>
    public ArtistLiteRow Create(
        Guid? id = null,
        string? name = null)
    {
        return new ArtistLiteRow
        {
            Id = id ?? _faker.Random.Guid(),
            Name = name ?? _faker.Name.FullName()
        };
    }

    /// <summary>
    /// Creates a list of <see cref="ArtistLiteRow"/>.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="ArtistLiteRow"/> instances.</returns>
    public List<ArtistLiteRow> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
