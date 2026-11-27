#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;

/// <summary>
/// Fixture class for the <see cref="BookArtworkEntity"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookArtworkEntityFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="BookArtworkEntity"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the artwork.</param>
    /// <param name="bookId">Optional. The Id of the book the artwork belongs to.</param>
    /// <param name="artworkType">Optional. The type of the artwork.</param>
    /// <param name="ordinal">Optional. The ordinal of the artwork within its type.</param>
    /// <param name="fileName">Optional. The relative file name of the stored artwork.</param>
    /// <param name="contentHash">Optional. The content hash of the stored artwork.</param>
    /// <param name="status">Optional. The status of the artwork enrichment.</param>
    /// <param name="provider">Optional. The name of the plugin that resolved the artwork.</param>
    /// <param name="lastUpdateUtc">Optional. The date and time when the artwork was last resolved.</param>
    /// <returns>The created <see cref="BookArtworkEntity"/>.</returns>
    public BookArtworkEntity Create(
        Guid? id = null,
        Guid? bookId = null,
        ArtworkType? artworkType = null,
        int? ordinal = null,
        string? fileName = null,
        ulong? contentHash = null,
        ArtworkStatus? status = null,
        string? provider = null,
        DateTime? lastUpdateUtc = null)
    {
        return new BookArtworkEntity
        {
            Id = id ?? Guid.NewGuid(),
            BookId = bookId ?? Guid.NewGuid(),
            ArtworkType = artworkType ?? ArtworkType.Cover,
            Ordinal = ordinal ?? 0,
            FileName = fileName ?? _faker.System.FilePath(),
            ContentHash = contentHash ?? (ulong)_faker.Random.ULong(),
            Status = status ?? _faker.PickRandom<ArtworkStatus>(),
            Provider = provider ?? _faker.Company.CompanyName(),
            LastUpdateUtc = lastUpdateUtc ?? _faker.Date.Recent(),
            CreatedOnUtc = _faker.Date.Past(),
            CreatedBy = Guid.NewGuid(),
            UpdatedOnUtc = null,
            UpdatedBy = null
        };
    }

    /// <summary>
    /// Creates a list of <see cref="BookArtworkEntity"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="BookArtworkEntity"/> instances.</returns>
    public List<BookArtworkEntity> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
