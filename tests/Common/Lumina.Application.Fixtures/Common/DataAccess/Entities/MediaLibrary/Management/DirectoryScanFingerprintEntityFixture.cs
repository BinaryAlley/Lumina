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
/// Fixture class for generating <see cref="DirectoryScanFingerprintEntity"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class DirectoryScanFingerprintEntityFixture
{
    /// <summary>
    /// Creates a new <see cref="DirectoryScanFingerprintEntity"/> instance with randomized test data.
    /// </summary>
    /// <param name="id">Optional. The Id of the directory scan fingerprint.</param>
    /// <param name="libraryId">Optional. The Id of the media library to which the fingerprint belongs.</param>
    /// <param name="path">Optional path of the directory to which the fingerprint belongs.</param>
    /// <param name="lastWriteTimeUtc">Optional last write time of the directory, in UTC.</param>
    /// <returns>A configured <see cref="DirectoryScanFingerprintEntity"/> instance.</returns>
    public DirectoryScanFingerprintEntity Create(
        Guid? id = null,
        Guid? libraryId = null,
        string? path = null,
        DateTime? lastWriteTimeUtc = null)
    {
        return new Faker<DirectoryScanFingerprintEntity>()
            .CustomInstantiator(faker => new DirectoryScanFingerprintEntity
            {
                Id = id ?? Guid.NewGuid(),
                LibraryId = libraryId ?? Guid.NewGuid(),
                Path = path ?? faker.System.DirectoryPath(),
                LastWriteTimeUtc = lastWriteTimeUtc ?? faker.Date.Recent()
            })
            .Generate();
    }

    /// <summary>
    /// Creates multiple <see cref="DirectoryScanFingerprintEntity"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="DirectoryScanFingerprintEntity"/> instances.</returns>
    public List<DirectoryScanFingerprintEntity> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
