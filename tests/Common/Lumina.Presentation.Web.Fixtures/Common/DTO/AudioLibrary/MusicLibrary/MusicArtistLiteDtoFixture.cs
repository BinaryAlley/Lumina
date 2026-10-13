#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.AudioLibrary.MusicLibrary;

/// <summary>
/// Fixture class for the <see cref="MusicArtistLiteDto"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicArtistLiteDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="MusicArtistLiteDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="id">Optional. The Id of the artist.</param>
    /// <param name="name">Optional. The name of the artist.</param>
    /// <param name="coverPath">Optional. The path of the image representing the artist.</param>
    /// <param name="includeCoverPath">Whether the cover path should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="MusicArtistLiteDto"/> instance.</returns>
    public MusicArtistLiteDto Create(
        Guid? id = null,
        string? name = null,
        string? coverPath = null,
        bool includeCoverPath = true)
    {
        return new MusicArtistLiteDto
        {
            Id = id ?? _faker.Random.Guid(),
            Name = name ?? _faker.Name.FullName(),
            CoverPath = includeCoverPath ? (coverPath ?? _faker.System.FilePath()) : null
        };
    }

    /// <summary>
    /// Creates multiple <see cref="MusicArtistLiteDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MusicArtistLiteDto"/> instances.</returns>
    public List<MusicArtistLiteDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
