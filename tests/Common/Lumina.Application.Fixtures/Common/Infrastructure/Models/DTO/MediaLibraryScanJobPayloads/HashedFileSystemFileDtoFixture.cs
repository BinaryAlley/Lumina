#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.Infrastructure.Models.DTO.MediaLibraryScanJobPayloads;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.Infrastructure.Models.DTO.MediaLibraryScanJobPayloads;

/// <summary>
/// Fixture class for the <see cref="HashedFileSystemFileDto"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class HashedFileSystemFileDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="HashedFileSystemFileDto"/>.
    /// </summary>
    /// <param name="path">Optional. The path of the file system file.</param>
    /// <param name="size">Optional. The size of the file system file.</param>
    /// <param name="currentHash">Optional. The current hash obtained by sampling the file system file contents.</param>
    /// <param name="oldHash">Optional. The old hash of the file system file, stored at the previous scan.</param>
    /// <param name="ticks">Optional. The time and date when the file system file was last modified, stored in ticks.</param>
    /// <returns>The created <see cref="HashedFileSystemFileDto"/>.</returns>
    public HashedFileSystemFileDto Create(
        string? path = null,
        long? size = null,
        ulong? currentHash = null,
        ulong? oldHash = null,
        long? ticks = null)
    {
        return new HashedFileSystemFileDto
        {
            Path = path ?? _faker.System.FilePath(),
            Size = size ?? _faker.Random.Long(1, 1_000_000),
            CurrentHash = currentHash ?? _faker.Random.ULong(),
            OldHash = oldHash ?? _faker.Random.ULong(),
            Ticks = ticks ?? _faker.Date.Recent().Ticks
        };
    }

    /// <summary>
    /// Creates a list of <see cref="HashedFileSystemFileDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="HashedFileSystemFileDto"/> instances.</returns>
    public List<HashedFileSystemFileDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
