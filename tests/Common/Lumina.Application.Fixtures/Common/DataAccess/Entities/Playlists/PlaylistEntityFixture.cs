#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.Playlists;
using Lumina.Domain.SharedKernel.Common.Enums.Playlists;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DataAccess.Entities.Playlists;

/// <summary>
/// Fixture class for the <see cref="PlaylistEntity"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaylistEntityFixture
{
    /// <summary>
    /// Creates a random valid <see cref="PlaylistEntity"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the playlist.</param>
    /// <param name="userId">Optional. The Id of the user that owns the playlist.</param>
    /// <param name="name">Optional. The name of the playlist.</param>
    /// <param name="includeDescription">Whether the description should be populated.</param>
    /// <param name="description">Optional. The description of the playlist.</param>
    /// <param name="type">Optional. The kind of media the playlist holds.</param>
    /// <param name="entries">Optional. The ordered entries of the playlist.</param>
    /// <param name="entryCount">Optional. The number of entries the playlist holds.</param>
    /// <param name="version">Optional. The concurrency token of the playlist.</param>
    /// <param name="createdOnUtc">Optional. The time and date when the playlist was created.</param>
    /// <param name="createdBy">Optional. The Id of the user that created the playlist.</param>
    /// <param name="includeUpdatedOnUtc">Whether the update time should be populated.</param>
    /// <param name="updatedOnUtc">Optional. The time and date when the playlist was updated.</param>
    /// <param name="updatedBy">Optional. The Id of the user that updated the playlist.</param>
    /// <returns>The created playlist entity.</returns>
    public PlaylistEntity Create(
        Guid? id = null,
        Guid? userId = null,
        string? name = null,
        bool includeDescription = true,
        string? description = null,
        PlaylistType? type = null,
        IEnumerable<PlaylistEntryEntity>? entries = null,
        int? entryCount = null,
        long? version = null,
        DateTime? createdOnUtc = null,
        Guid? createdBy = null,
        bool includeUpdatedOnUtc = false,
        DateTime? updatedOnUtc = null,
        Guid? updatedBy = null)
    {
        Guid playlistId = id ?? Guid.NewGuid();
        List<PlaylistEntryEntity> resolvedEntries = entries is null ? [] : [.. entries];

        return new Faker<PlaylistEntity>()
            .CustomInstantiator(f => new PlaylistEntity
            {
                Id = playlistId,
                UserId = userId ?? f.Random.Guid(),
                Name = name ?? f.Lorem.Sentence(3),
                Description = includeDescription ? (description ?? f.Lorem.Sentence()) : null,
                Type = type ?? f.PickRandom<PlaylistType>(),
                Entries = resolvedEntries,
                EntryCount = entryCount ?? resolvedEntries.Count,
                Version = version ?? f.Random.Long(0, 1_000L),
                CreatedOnUtc = createdOnUtc ?? f.Date.Past(),
                CreatedBy = createdBy ?? f.Random.Guid(),
                UpdatedOnUtc = includeUpdatedOnUtc ? (updatedOnUtc ?? f.Date.Recent()) : null,
                UpdatedBy = includeUpdatedOnUtc ? (updatedBy ?? f.Random.Guid()) : null
            })
            .Generate();
    }

    /// <summary>
    /// Creates a list of <see cref="PlaylistEntity"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <param name="userId">Optional. The Id of the user that owns the created playlists.</param>
    /// <returns>List of configured <see cref="PlaylistEntity"/> instances.</returns>
    public List<PlaylistEntity> CreateMany(int count = 3, Guid? userId = null)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create(userId: userId))];
    }
}
