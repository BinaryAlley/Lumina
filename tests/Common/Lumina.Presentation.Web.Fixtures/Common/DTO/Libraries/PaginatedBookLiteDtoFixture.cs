#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.Libraries;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.Libraries;

/// <summary>
/// Fixture class for generating <see cref="PaginatedBookLiteDto"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class PaginatedBookLiteDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="PaginatedBookLiteDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="data">Optional collection of lightweight book elements.</param>
    /// <param name="currentPage">Optional current page number in a paginated collection.</param>
    /// <param name="perPage">Optional number of items to include per page in paginated results.</param>
    /// <param name="count">Optional total number of elements available in the paginated result set.</param>
    /// <param name="numberOfPages">Optional total number of pages based on the elements available and per page.</param>
    /// <returns>A configured <see cref="PaginatedBookLiteDto"/> instance.</returns>
    public PaginatedBookLiteDto Create(
        IReadOnlyList<BookLiteDto>? data = null,
        int? currentPage = null,
        int? perPage = null,
        int? count = null,
        int? numberOfPages = null)
    {
        return new PaginatedBookLiteDto
        {
            Data = data ?? [],
            CurrentPage = currentPage ?? _faker.Random.Int(1, 10),
            PerPage = perPage ?? _faker.Random.Int(1, 100),
            Count = count ?? _faker.Random.Int(0, 1000),
            NumberOfPages = numberOfPages ?? _faker.Random.Int(1, 100)
        };
    }

    /// <summary>
    /// Creates multiple <see cref="PaginatedBookLiteDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="PaginatedBookLiteDto"/> instances.</returns>
    public List<PaginatedBookLiteDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
