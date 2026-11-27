#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.Playlists;
using Lumina.Presentation.Web.Common.Enums.Playlists;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.Playlists;

/// <summary>
/// Fixture class for the <see cref="PlaylistDetailsDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaylistDetailsDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="PlaylistDetailsDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="id">Optional Id of the playlist.</param>
    /// <param name="name">Optional name of the playlist.</param>
    /// <param name="description">Optional description of the playlist.</param>
    /// <param name="type">Optional kind of media the playlist holds.</param>
    /// <param name="updatedOnUtc">Optional date and time when the playlist was last updated.</param>
    /// <param name="includeDescription">Whether the description should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeUpdatedOnUtc">Whether the last update date should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="PlaylistDetailsDto"/> instance.</returns>
    public PlaylistDetailsDto Create(
        Guid? id = null,
        string? name = null,
        string? description = null,
        PlaylistType? type = null,
        DateTime? updatedOnUtc = null,
        bool includeDescription = true,
        bool includeUpdatedOnUtc = true)
    {
        DateTime resolvedUpdatedOnUtc = DateTime.SpecifyKind(updatedOnUtc ?? DateTime.UtcNow.AddHours(-_faker.Random.Int(1, 24)), DateTimeKind.Utc);
        return new PlaylistDetailsDto(
            Id: id ?? _faker.Random.Guid(),
            Name: name ?? _faker.Lorem.Sentence(3),
            Description: includeDescription ? (description ?? _faker.Lorem.Sentence(5)) : null,
            Type: type ?? PlaylistType.Music,
            UpdatedOnUtc: includeUpdatedOnUtc ? resolvedUpdatedOnUtc : null
        );
    }

    /// <summary>
    /// Creates multiple <see cref="PlaylistDetailsDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="PlaylistDetailsDto"/> instances.</returns>
    public List<PlaylistDetailsDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
