#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;

/// <summary>
/// Fixture class for generating <see cref="LibraryScanResultEntity"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryScanResultEntityFixture
{
    /// <summary>
    /// Creates a new <see cref="LibraryScanResultEntity"/> instance with randomized test data.
    /// </summary>
    /// <param name="id">Optional. The Id of the media library scan result.</param>
    /// <param name="libraryScanId">Optional. The Id of the media library scan that this result belongs to.</param>
    /// <param name="status">Optional status of the media library scan file.</param>
    /// <param name="path">Optional path of the media library scan file.</param>
    /// <param name="contentHash">Optional hash calculated for the media library scan file.</param>
    /// <param name="fileSize">Optional size of the media library scan file.</param>
    /// <param name="ticks">Optional last write time of the media library scan file, stored in ticks.</param>
    /// <returns>A configured <see cref="LibraryScanResultEntity"/> instance.</returns>
    public LibraryScanResultEntity Create(
        Guid? id = null,
        Guid? libraryScanId = null,
        LibraryScanFileStatus? status = null,
        string? path = null,
        ulong? contentHash = null,
        long? fileSize = null,
        long? ticks = null)
    {
        return new Faker<LibraryScanResultEntity>()
            .CustomInstantiator(faker => new LibraryScanResultEntity
            {
                Id = id ?? Guid.NewGuid(),
                LibraryScanId = libraryScanId ?? Guid.NewGuid(),
                Status = status ?? faker.PickRandom<LibraryScanFileStatus>(),
                Path = path ?? faker.System.FilePath(),
                ContentHash = contentHash ?? faker.Random.ULong(),
                FileSize = fileSize ?? faker.Random.Long(1, long.MaxValue),
                Ticks = ticks ?? faker.Random.Long(0, long.MaxValue),
                LibraryScan = null!
            })
            .Generate();
    }

    /// <summary>
    /// Creates multiple <see cref="LibraryScanResultEntity"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="LibraryScanResultEntity"/> instances.</returns>
    public List<LibraryScanResultEntity> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
