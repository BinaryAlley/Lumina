#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.DTO.Common;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.DTO.Common;

/// <summary>
/// Fixture class for the <see cref="GenreDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GenreDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="GenreDto"/>.
    /// </summary>
    /// <param name="name">Optional. The name of the genre.</param>
    /// <param name="includeName">Whether the name should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="GenreDto"/>.</returns>
    public GenreDto Create(
        string? name = null, 
        bool includeName = true)
    {
        return new GenreDto(includeName ? name ?? _faker.Lorem.Word() : null);
    }

    /// <summary>
    /// Creates a list of <see cref="GenreDto"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GenreDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
