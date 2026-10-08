#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;

/// <summary>
/// Fixture class for the <see cref="MusicArtworkEntity"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicArtworkEntityFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="MusicArtworkEntity"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the artwork.</param>
    /// <param name="ownerType">Optional. The type of the music library item that owns the artwork.</param>
    /// <param name="ownerId">Optional. The Id of the music library item that owns the artwork.</param>
    /// <param name="artworkType">Optional. The type of the artwork.</param>
    /// <param name="ordinal">Optional. The ordinal of the artwork within its type.</param>
    /// <param name="fileName">Optional. The relative file name of the stored artwork.</param>
    /// <param name="contentHash">Optional. The content hash of the stored artwork.</param>
    /// <param name="status">Optional. The status of the artwork enrichment.</param>
    /// <param name="provider">Optional. The name of the plugin that resolved the artwork.</param>
    /// <param name="lastUpdateUtc">Optional. The date and time when the artwork was last resolved.</param>
    /// <param name="includeFileName">Whether the file name should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeProvider">Whether the provider should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeLastUpdateUtc">Whether the last update date should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="MusicArtworkEntity"/>.</returns>
    public MusicArtworkEntity Create(
        Guid? id = null,
        MusicArtworkOwnerType? ownerType = null,
        Guid? ownerId = null,
        ArtworkType? artworkType = null,
        int? ordinal = null,
        string? fileName = null,
        ulong? contentHash = null,
        ArtworkStatus? status = null,
        string? provider = null,
        DateTime? lastUpdateUtc = null,
        bool includeFileName = true,
        bool includeProvider = true,
        bool includeLastUpdateUtc = true)
    {
        return new MusicArtworkEntity
        {
            Id = id ?? Guid.NewGuid(),
            OwnerType = ownerType ?? _faker.PickRandom<MusicArtworkOwnerType>(),
            OwnerId = ownerId ?? Guid.NewGuid(),
            ArtworkType = artworkType ?? _faker.PickRandom<ArtworkType>(),
            Ordinal = ordinal ?? _faker.Random.Int(0, 5),
            FileName = includeFileName ? (fileName ?? _faker.System.FilePath()) : null,
            ContentHash = contentHash ?? (ulong)_faker.Random.ULong(),
            Status = status ?? _faker.PickRandom<ArtworkStatus>(),
            Provider = includeProvider ? (provider ?? _faker.Company.CompanyName()) : null,
            LastUpdateUtc = includeLastUpdateUtc ? (lastUpdateUtc ?? _faker.Date.Recent()) : null,
            CreatedOnUtc = _faker.Date.Past(),
            CreatedBy = Guid.NewGuid(),
            UpdatedOnUtc = null,
            UpdatedBy = null
        };
    }

    /// <summary>
    /// Creates a list of <see cref="MusicArtworkEntity"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MusicArtworkEntity"/> instances.</returns>
    public List<MusicArtworkEntity> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
