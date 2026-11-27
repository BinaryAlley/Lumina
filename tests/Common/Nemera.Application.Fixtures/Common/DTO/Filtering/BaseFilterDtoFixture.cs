#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DTO.Filtering;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DTO.Filtering;

/// <summary>
/// Fixture class for the <see cref="BaseFilterDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class BaseFilterDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="BaseFilterDto"/>.
    /// </summary>
    /// <param name="searchTerm">Optional. The search term used to filter results.</param>
    /// <returns>The created <see cref="BaseFilterDto"/>.</returns>
    public BaseFilterDto Create(string? searchTerm = null)
    {
        return new BaseFilterDto
        {
            SearchTerm = searchTerm ?? _faker.Lorem.Sentence()
        };
    }

    /// <summary>
    /// Creates a list of <see cref="BaseFilterDto"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<BaseFilterDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
