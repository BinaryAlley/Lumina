#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Fixtures.Responses.Common;
using Lumina.Contracts.Responses.Common;
using Lumina.Contracts.Responses.Playlists;
using Lumina.Domain.SharedKernel.Common.Enums.Playlists;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Responses.Playlists;

/// <summary>
/// Fixture class for the <see cref="PlaylistResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaylistResponseFixture
{
    private const int DEFAULT_ENTRY_COUNT = 3;

    private readonly Faker _faker = new();
    private readonly PlaylistEntryResponseFixture _playlistEntryResponseFixture = new();
    private readonly PaginatedResponseFixture<PlaylistEntryResponse> _paginatedResponseFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="PlaylistResponse"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the playlist.</param>
    /// <param name="name">Optional. The name of the playlist.</param>
    /// <param name="description">Optional. The description of the playlist.</param>
    /// <param name="type">Optional. The kind of media the playlist holds.</param>
    /// <param name="entries">Optional. The page of entries of the playlist.</param>
    /// <param name="createdOnUtc">Optional. The date and time when the playlist was created.</param>
    /// <param name="updatedOnUtc">Optional. The date and time when the playlist was last updated.</param>
    /// <param name="includeDescription">Whether the description should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeUpdatedOnUtc">Whether the update time should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="PlaylistResponse"/>.</returns>
    public PlaylistResponse Create(
        Guid? id = null,
        string? name = null,
        string? description = null,
        PlaylistType? type = null,
        PaginatedResponse<PlaylistEntryResponse>? entries = null,
        DateTime? createdOnUtc = null,
        DateTime? updatedOnUtc = null,
        bool includeDescription = true,
        bool includeUpdatedOnUtc = true)
    {
        return new PlaylistResponse(
            id ?? _faker.Random.Guid(),
            name ?? _faker.Lorem.Sentence(3),
            includeDescription ? (description ?? _faker.Lorem.Sentence()) : null,
            type ?? PlaylistType.Music,
            entries ?? CreateEntriesPage(),
            createdOnUtc ?? DateTime.UtcNow.AddDays(-_faker.Random.Int(1, 30)),
            includeUpdatedOnUtc ? (updatedOnUtc ?? DateTime.UtcNow.AddMinutes(-_faker.Random.Int(1, 1_440))) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="PlaylistResponse"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="PlaylistResponse"/> instances.</returns>
    public List<PlaylistResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }

    /// <summary>
    /// Creates a page of random playlist entries.
    /// </summary>
    /// <returns>The created page of playlist entries.</returns>
    private PaginatedResponse<PlaylistEntryResponse> CreateEntriesPage()
    {
        List<PlaylistEntryResponse> entries = [];
        for (int index = 0; index < DEFAULT_ENTRY_COUNT; index++)
            entries.Add(_playlistEntryResponseFixture.Create(position: index));

        return _paginatedResponseFixture.Create(data: entries, currentPage: 1, perPage: DEFAULT_ENTRY_COUNT, count: DEFAULT_ENTRY_COUNT, numberOfPages: 1);
    }
}
