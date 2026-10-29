#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.Playlists;
using Lumina.Application.Common.DTO.Playlists;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Playlists;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DTO.Playlists;

/// <summary>
/// Fixture class for the <see cref="PlaylistEntryDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaylistEntryDtoFixture
{
    private readonly Faker _faker = new();
    private readonly PlaylistEntryEntityFixture _playlistEntryEntityFixture = new();
    private readonly PlaylistEntryDisplayDtoFixture _playlistEntryDisplayDtoFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="PlaylistEntryDto"/>.
    /// </summary>
    /// <param name="entry">Optional. The stored playlist entry.</param>
    /// <param name="display">Optional. The display data of the referenced media item.</param>
    /// <param name="displayPosition">Optional. The dense zero based position shown to the client.</param>
    /// <param name="includeDisplay">Whether the display data should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="PlaylistEntryDto"/>.</returns>
    public PlaylistEntryDto Create(
        PlaylistEntryEntity? entry = null,
        PlaylistEntryDisplayDto? display = null,
        int? displayPosition = null,
        bool includeDisplay = true)
    {
        return new PlaylistEntryDto(
            entry ?? _playlistEntryEntityFixture.Create(),
            includeDisplay ? (display ?? _playlistEntryDisplayDtoFixture.Create()) : null,
            displayPosition ?? _faker.Random.Int(0, 1_000));
    }

    /// <summary>
    /// Creates a list of <see cref="PlaylistEntryDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="PlaylistEntryDto"/> instances.</returns>
    public List<PlaylistEntryDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
