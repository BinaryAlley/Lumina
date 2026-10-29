#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.Playlists;
using Lumina.Presentation.Web.Common.Enums.Playlists;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.Playlists;

/// <summary>
/// Fixture class for the <see cref="PlaylistDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaylistDtoFixture
{
    private readonly Faker _faker = new();
    private readonly PlaylistEntryDtoFixture _playlistEntryDtoFixture = new();
    private readonly PaginatedDtoFixture<PlaylistEntryDto> _paginatedDtoFixture = new();

    /// <summary>
    /// Creates a new <see cref="PlaylistDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="id">Optional Id of the playlist.</param>
    /// <param name="name">Optional name of the playlist.</param>
    /// <param name="description">Optional description of the playlist.</param>
    /// <param name="type">Optional kind of media the playlist holds.</param>
    /// <param name="entries">Optional page of ordered entries of the playlist.</param>
    /// <param name="createdOnUtc">Optional date and time when the playlist was created.</param>
    /// <param name="updatedOnUtc">Optional date and time when the playlist was last updated.</param>
    /// <param name="includeDescription">Whether the description should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeUpdatedOnUtc">Whether the last update date should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="PlaylistDto"/> instance.</returns>
    public PlaylistDto Create(
        Guid? id = null,
        string? name = null,
        string? description = null,
        PlaylistType? type = null,
        PaginatedDto<PlaylistEntryDto>? entries = null,
        DateTime? createdOnUtc = null,
        DateTime? updatedOnUtc = null,
        bool includeDescription = true,
        bool includeUpdatedOnUtc = true)
    {
        DateTime resolvedCreatedOnUtc = DateTime.SpecifyKind(createdOnUtc ?? DateTime.UtcNow.AddDays(-_faker.Random.Int(1, 30)), DateTimeKind.Utc);
        DateTime resolvedUpdatedOnUtc = DateTime.SpecifyKind(updatedOnUtc ?? resolvedCreatedOnUtc.AddHours(_faker.Random.Int(1, 24)), DateTimeKind.Utc);
        return new PlaylistDto(
            Id: id ?? _faker.Random.Guid(),
            Name: name ?? _faker.Lorem.Sentence(3),
            Description: includeDescription ? (description ?? _faker.Lorem.Sentence(5)) : null,
            Type: type ?? PlaylistType.Music,
            Entries: entries ?? _paginatedDtoFixture.Create(data: _playlistEntryDtoFixture.CreateMany(_faker.Random.Int(0, 5))),
            CreatedOnUtc: resolvedCreatedOnUtc,
            UpdatedOnUtc: includeUpdatedOnUtc ? resolvedUpdatedOnUtc : null
        );
    }

    /// <summary>
    /// Creates multiple <see cref="PlaylistDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="PlaylistDto"/> instances.</returns>
    public List<PlaylistDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
