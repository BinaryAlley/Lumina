#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.Common;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.Common;

/// <summary>
/// Fixture class for the <see cref="PaginatedDto{T}"/> class.
/// </summary>
/// <typeparam name="T">The type of the elements of the page.</typeparam>
[ExcludeFromCodeCoverage]
public class PaginatedDtoFixture<T>
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="PaginatedDto{T}"/> instance with randomized test data.
    /// </summary>
    /// <param name="data">Optional collection of elements of the page.</param>
    /// <param name="currentPage">Optional current page number in a paginated collection.</param>
    /// <param name="perPage">Optional number of items to include per page in paginated results.</param>
    /// <param name="count">Optional total number of elements available in the paginated result set.</param>
    /// <param name="numberOfPages">Optional total number of pages based on the elements available and per page.</param>
    /// <returns>A configured <see cref="PaginatedDto{T}"/> instance.</returns>
    public PaginatedDto<T> Create(
        IReadOnlyList<T>? data = null,
        int? currentPage = null,
        int? perPage = null,
        int? count = null,
        int? numberOfPages = null)
    {
        return new PaginatedDto<T>(
            Data: data is null ? [] : [.. data],
            CurrentPage: currentPage ?? _faker.Random.Int(1, 10),
            PerPage: perPage ?? _faker.Random.Int(1, 100),
            Count: count ?? _faker.Random.Int(0, 1000),
            NumberOfPages: numberOfPages ?? _faker.Random.Int(1, 100));
    }

    /// <summary>
    /// Creates multiple <see cref="PaginatedDto{T}"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="PaginatedDto{T}"/> instances.</returns>
    public List<PaginatedDto<T>> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
