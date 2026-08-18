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
/// Fixture class for generating <see cref="LibraryScanSnapshotEntity"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryScanSnapshotEntityFixture
{
    /// <summary>
    /// Creates a new <see cref="LibraryScanSnapshotEntity"/> instance with randomized test data.
    /// </summary>
    /// <param name="id">Optional. The Id of the media library scan snapshot item.</param>
    /// <param name="libraryId">Optional. The Id of the media library to which the snapshot item belongs.</param>
    /// <param name="path">Optional path of the media library scan snapshot item.</param>
    /// <param name="contentHash">Optional hash calculated for the media library scan snapshot item.</param>
    /// <param name="fileSize">Optional size of the media library scan snapshot item.</param>
    /// <param name="ticks">Optional last write time of the media library scan snapshot item, stored in ticks.</param>
    /// <returns>A configured <see cref="LibraryScanSnapshotEntity"/> instance.</returns>
    public LibraryScanSnapshotEntity Create(
        Guid? id = null,
        Guid? libraryId = null,
        string? path = null,
        ulong? contentHash = null,
        long? fileSize = null,
        long? ticks = null)
    {
        return new Faker<LibraryScanSnapshotEntity>()
            .CustomInstantiator(faker => new LibraryScanSnapshotEntity
            {
                Id = id ?? Guid.NewGuid(),
                LibraryId = libraryId ?? Guid.NewGuid(),
                Path = path ?? faker.System.FilePath(),
                ContentHash = contentHash ?? faker.Random.ULong(),
                FileSize = fileSize ?? faker.Random.Long(1, long.MaxValue),
                Ticks = ticks ?? faker.Random.Long(0, long.MaxValue),
                Library = null!,
                CreatedOnUtc = faker.Date.Past(),
                CreatedBy = Guid.NewGuid(),
                UpdatedOnUtc = faker.Random.Bool() ? faker.Date.Recent() : null,
                UpdatedBy = faker.Random.Bool() ? Guid.NewGuid() : null
            })
            .Generate();
    }

    /// <summary>
    /// Creates multiple <see cref="LibraryScanSnapshotEntity"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="LibraryScanSnapshotEntity"/> instances.</returns>
    public List<LibraryScanSnapshotEntity> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
