#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;

/// <summary>
/// Fixture class for generating <see cref="LibraryScanStagingResultsEntity"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryScanStagingResultsEntityFixture
{
    /// <summary>
    /// Creates a new <see cref="LibraryScanStagingResultsEntity"/> instance with randomized test data.
    /// </summary>
    /// <param name="id">Optional. The Id of the media library scan staging result.</param>
    /// <param name="libraryScanId">Optional. The Id of the media library scan that this staging result belongs to.</param>
    /// <param name="path">Optional path of the media library scan staging result.</param>
    /// <param name="size">Optional size of the media library scan staging result.</param>
    /// <param name="ticks">Optional last write time of the file system item, stored in ticks.</param>
    /// <param name="contentHash">Optional hash calculated for the media library scan staging result.</param>
    /// <param name="previousContentHash">Optional hash stored in the media library scan snapshot of a previous scan.</param>
    /// <param name="needsRehash">Optional value indicating whether the file system item needs its content hashed.</param>
    /// <param name="isNew">Optional value indicating whether the file system item is new.</param>
    /// <returns>A configured <see cref="LibraryScanStagingResultsEntity"/> instance.</returns>
    public LibraryScanStagingResultsEntity Create(
        Guid? id = null,
        Guid? libraryScanId = null,
        string? path = null,
        long? size = null,
        long? ticks = null,
        ulong? contentHash = null,
        ulong? previousContentHash = null,
        bool? needsRehash = null,
        bool? isNew = null)
    {
        return new Faker<LibraryScanStagingResultsEntity>()
            .CustomInstantiator(faker => new LibraryScanStagingResultsEntity
            {
                Id = id ?? Guid.NewGuid(),
                LibraryScanId = libraryScanId ?? Guid.NewGuid(),
                Path = path ?? faker.System.FilePath(),
                Size = size ?? faker.Random.Long(1, long.MaxValue),
                Ticks = ticks ?? faker.Random.Long(0, long.MaxValue),
                ContentHash = contentHash ?? faker.Random.ULong(),
                PreviousContentHash = previousContentHash ?? faker.Random.ULong(),
                NeedsRehash = needsRehash ?? faker.Random.Bool(),
                IsNew = isNew ?? faker.Random.Bool()
            })
            .Generate();
    }

    /// <summary>
    /// Creates multiple <see cref="LibraryScanStagingResultsEntity"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="LibraryScanStagingResultsEntity"/> instances.</returns>
    public List<LibraryScanStagingResultsEntity> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
