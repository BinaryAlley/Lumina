#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Responses.Common;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Responses.Common;

/// <summary>
/// Fixture class for the <see cref="PaginatedResponse{TData}"/> record.
/// </summary>
/// <typeparam name="TData">The type of the data items contained in the paginated response.</typeparam>
[ExcludeFromCodeCoverage]
public class PaginatedResponseFixture<TData>
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="PaginatedResponse{TData}"/>.
    /// </summary>
    /// <param name="data">Optional. The collection of data items.</param>
    /// <param name="currentPage">Optional. The current page number.</param>
    /// <param name="perPage">Optional. The number of items per page.</param>
    /// <param name="count">Optional. The total number of elements available.</param>
    /// <param name="numberOfPages">Optional. The total number of pages.</param>
    /// <returns>The created <see cref="PaginatedResponse{TData}"/>.</returns>
    public PaginatedResponse<TData> Create(
        IReadOnlyList<TData>? data = null,
        int? currentPage = null,
        int? perPage = null,
        int? count = null,
        int? numberOfPages = null)
    {
        return new PaginatedResponse<TData>
        {
            Data = data ?? [],
            CurrentPage = currentPage ?? _faker.Random.Int(1, 10),
            PerPage = perPage ?? _faker.Random.Int(1, 100),
            Count = count ?? _faker.Random.Int(0, 1000),
            NumberOfPages = numberOfPages ?? _faker.Random.Int(1, 100)
        };
    }

    /// <summary>
    /// Creates a list of <see cref="PaginatedResponse{TData}"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="PaginatedResponse{TData}"/> instances.</returns>
    public List<PaginatedResponse<TData>> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
