#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using Lumina.Domain.SharedKernel.Common.Enums.Playback;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Playback;

/// <summary>
/// Fixture class for the <see cref="PlaybackDeliveryHandleDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaybackDeliveryHandleDtoFixture
{
    private readonly Faker _faker = new();
    private readonly NormalizedTrackRequestDtoFixture _normalizedTrackRequestDtoFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="PlaybackDeliveryHandleDto"/>.
    /// </summary>
    /// <param name="request">Optional. The complete normalization request the delivery declares.</param>
    /// <param name="shape">Optional. The shape the delivered audio is addressed by.</param>
    /// <param name="libraryId">Optional. The Id of the library the track belongs to.</param>
    /// <param name="albumId">Optional. The Id of the album the track belongs to.</param>
    /// <param name="artistId">Optional. The Id of the artist that owns the album.</param>
    /// <returns>The created <see cref="PlaybackDeliveryHandleDto"/>.</returns>
    public PlaybackDeliveryHandleDto Create(
        NormalizedTrackRequestDto? request = null,
        PlaybackDeliveryShape? shape = null,
        Guid? libraryId = null,
        Guid? albumId = null,
        Guid? artistId = null)
    {
        return new PlaybackDeliveryHandleDto(
            request ?? _normalizedTrackRequestDtoFixture.Create(),
            shape ?? _faker.PickRandom<PlaybackDeliveryShape>(),
            libraryId ?? _faker.Random.Guid(),
            albumId ?? _faker.Random.Guid(),
            artistId ?? _faker.Random.Guid());
    }

    /// <summary>
    /// Creates a list of <see cref="PlaybackDeliveryHandleDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="PlaybackDeliveryHandleDto"/> instances.</returns>
    public List<PlaybackDeliveryHandleDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
