#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.Playlists;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.Playlists;

/// <summary>
/// Fixture class for the <see cref="PlaylistEntryAddedDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaylistEntryAddedDtoFixture
{
    private readonly Faker _faker = new();
    private readonly PlaylistEntryDtoFixture _playlistEntryDtoFixture = new();

    /// <summary>
    /// Creates a new <see cref="PlaylistEntryAddedDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="playlistId">Optional Id of the playlist the entry was added to.</param>
    /// <param name="entry">Optional added entry.</param>
    /// <param name="updatedOnUtc">Optional date and time when the playlist was last updated.</param>
    /// <param name="includeUpdatedOnUtc">Whether the last update date should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="PlaylistEntryAddedDto"/> instance.</returns>
    public PlaylistEntryAddedDto Create(
        Guid? playlistId = null,
        PlaylistEntryDto? entry = null,
        DateTime? updatedOnUtc = null,
        bool includeUpdatedOnUtc = true)
    {
        DateTime resolvedUpdatedOnUtc = DateTime.SpecifyKind(updatedOnUtc ?? DateTime.UtcNow.AddHours(-_faker.Random.Int(1, 24)), DateTimeKind.Utc);
        return new PlaylistEntryAddedDto(
            PlaylistId: playlistId ?? _faker.Random.Guid(),
            Entry: entry ?? _playlistEntryDtoFixture.Create(),
            UpdatedOnUtc: includeUpdatedOnUtc ? resolvedUpdatedOnUtc : null
        );
    }

    /// <summary>
    /// Creates multiple <see cref="PlaylistEntryAddedDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="PlaylistEntryAddedDto"/> instances.</returns>
    public List<PlaylistEntryAddedDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
