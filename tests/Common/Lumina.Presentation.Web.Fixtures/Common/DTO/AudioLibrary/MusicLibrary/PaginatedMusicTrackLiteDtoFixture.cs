#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.AudioLibrary.MusicLibrary;

/// <summary>
/// Fixture class for the <see cref="PaginatedMusicTrackLiteDto"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PaginatedMusicTrackLiteDtoFixture
{
    private readonly Faker _faker = new();
    private readonly MusicTrackLiteDtoFixture _musicTrackLiteDtoFixture = new();

    /// <summary>
    /// Creates a new <see cref="PaginatedMusicTrackLiteDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="data">Optional. The collection of lightweight track elements.</param>
    /// <param name="currentPage">Optional. The current page number in the paginated collection.</param>
    /// <param name="perPage">Optional. The number of items to include per page.</param>
    /// <param name="count">Optional. The total number of elements available in the result set.</param>
    /// <param name="numberOfPages">Optional. The total number of pages available.</param>
    /// <returns>A configured <see cref="PaginatedMusicTrackLiteDto"/> instance.</returns>
    public PaginatedMusicTrackLiteDto Create(
        IReadOnlyList<MusicTrackLiteDto>? data = null,
        int? currentPage = null,
        int? perPage = null,
        int? count = null,
        int? numberOfPages = null)
    {
        return new PaginatedMusicTrackLiteDto
        {
            Data = data ?? [.. _musicTrackLiteDtoFixture.CreateMany(_faker.Random.Number(1, 3))],
            CurrentPage = currentPage ?? _faker.Random.Int(1, 10),
            PerPage = perPage ?? _faker.Random.Int(1, 100),
            Count = count ?? _faker.Random.Int(0, 1000),
            NumberOfPages = numberOfPages ?? _faker.Random.Int(1, 100)
        };
    }

    /// <summary>
    /// Creates multiple <see cref="PaginatedMusicTrackLiteDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="PaginatedMusicTrackLiteDto"/> instances.</returns>
    public List<PaginatedMusicTrackLiteDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
