#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.Common;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DataAccess.Entities.Common;

/// <summary>
/// Fixture class for the <see cref="GenreEntity"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GenreEntityFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="GenreEntity"/>.
    /// </summary>
    /// <param name="name">Optional. The name of the genre.</param>
    /// <param name="includeName">Whether the name should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="GenreEntity"/>.</returns>
    public GenreEntity Create(
        string? name = null, 
        bool includeName = true)
    {
        return new GenreEntity(includeName ? name ?? _faker.Lorem.Word() : null);
    }

    /// <summary>
    /// Creates a list of <see cref="GenreEntity"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GenreEntity> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
