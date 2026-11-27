#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DTO.Pagination;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DTO.Pagination;

/// <summary>
/// Fixture class for the <see cref="PaginationDataDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class PaginationDataDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="PaginationDataDto"/>.
    /// </summary>
    /// <param name="currentPage">Optional. The current page number in a paginated collection.</param>
    /// <param name="perPage">Optional. The number of items to include per page in paginated results.</param>
    /// <returns>The created <see cref="PaginationDataDto"/>.</returns>
    public PaginationDataDto Create(
        int? currentPage = null, 
        int? perPage = null)
    {
        return new PaginationDataDto
        {
            CurrentPage = currentPage ?? _faker.Random.Int(1, 100),
            PerPage = perPage ?? _faker.Random.Int(1, 200)
        };
    }

    /// <summary>
    /// Creates a list of <see cref="PaginationDataDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="PaginationDataDto"/> instances.</returns>
    public List<PaginationDataDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
