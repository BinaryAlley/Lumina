#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Queries.GetArtistsLite;
using Lumina.Application.Fixtures.Common.DTO.Pagination;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Queries.GetArtistsLite;

/// <summary>
/// Fixture class for the <see cref="GetArtistsLiteQuery"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetArtistsLiteQueryFixture
{
    private readonly Faker _faker = new();
    private readonly PaginationDataDtoFixture _paginationDataDtoFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="GetArtistsLiteQuery"/>.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the media library whose artists are retrieved, taken from the route.</param>
    /// <param name="paginationData">Optional. The pagination data of the query.</param>
    /// <param name="searchTerm">Optional. The search term used to filter the artists by name.</param>
    /// <param name="includeLibraryId">Whether the library Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includePaginationData">Whether the pagination data should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeSearchTerm">Whether the search term should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="GetArtistsLiteQuery"/>.</returns>
    public GetArtistsLiteQuery Create(
        string? libraryId = null,
        PaginationDataDto? paginationData = null,
        string? searchTerm = null,
        bool includeLibraryId = true,
        bool includePaginationData = true,
        bool includeSearchTerm = true)
    {
        return new GetArtistsLiteQuery(
            includeLibraryId ? (libraryId ?? Guid.NewGuid().ToString()) : null,
            includePaginationData ? (paginationData ?? _paginationDataDtoFixture.Create()) : null,
            includeSearchTerm ? (searchTerm ?? _faker.Name.FullName()) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="GetArtistsLiteQuery"/>.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="GetArtistsLiteQuery"/> instances.</returns>
    public List<GetArtistsLiteQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
