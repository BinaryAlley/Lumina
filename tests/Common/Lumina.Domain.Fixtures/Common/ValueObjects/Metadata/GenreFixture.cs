#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Common.ValueObjects.Metadata;

/// <summary>
/// Fixture class for the <see cref="Genre"/> domain value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class GenreFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="Genre"/>.
    /// </summary>
    /// <param name="name">Optional. The name of the genre. If not provided, a random name is generated.</param>
    /// <returns>The created <see cref="Genre"/>.</returns>
    public Genre Create(string? name = null)
    {
        Result<Genre> genreResult = Genre.Create(name ?? _faker.Lorem.Word());

        if (genreResult.IsFailure)
            throw new InvalidOperationException("Failed to create Genre: " + string.Join(", ", genreResult.Errors));
        return genreResult.Value;
    }

    /// <summary>
    /// Creates multiple <see cref="Genre"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="Genre"/> instances.</returns>
    public List<Genre> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
