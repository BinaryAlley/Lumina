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
/// Fixture class for the <see cref="PlaylistEntryMovedDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaylistEntryMovedDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="PlaylistEntryMovedDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="playlistId">Optional Id of the playlist whose entry was moved.</param>
    /// <param name="entryId">Optional Id of the moved entry.</param>
    /// <param name="position">Optional new zero based position of the entry.</param>
    /// <param name="updatedOnUtc">Optional date and time when the playlist was last updated.</param>
    /// <param name="includeUpdatedOnUtc">Whether the last update date should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="PlaylistEntryMovedDto"/> instance.</returns>
    public PlaylistEntryMovedDto Create(
        Guid? playlistId = null,
        Guid? entryId = null,
        int? position = null,
        DateTime? updatedOnUtc = null,
        bool includeUpdatedOnUtc = true)
    {
        DateTime resolvedUpdatedOnUtc = DateTime.SpecifyKind(updatedOnUtc ?? DateTime.UtcNow.AddHours(-_faker.Random.Int(1, 24)), DateTimeKind.Utc);
        return new PlaylistEntryMovedDto(
            PlaylistId: playlistId ?? _faker.Random.Guid(),
            EntryId: entryId ?? _faker.Random.Guid(),
            Position: position ?? _faker.Random.Int(0, 1_000),
            UpdatedOnUtc: includeUpdatedOnUtc ? resolvedUpdatedOnUtc : null
        );
    }

    /// <summary>
    /// Creates multiple <see cref="PlaylistEntryMovedDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="PlaylistEntryMovedDto"/> instances.</returns>
    public List<PlaylistEntryMovedDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
