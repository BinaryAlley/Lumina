#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.Common;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.Common;

/// <summary>
/// Fixture class for generating <see cref="GenreDto"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class GenreDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="GenreDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="name">Optional. The name of the genre.</param>
    /// <returns>A configured <see cref="GenreDto"/> instance.</returns>
    public GenreDto Create(string? name = null)
    {
        return new GenreDto
        {
            Name = name ?? _faker.Lorem.Word()
        };
    }

    /// <summary>
    /// Creates multiple <see cref="GenreDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="GenreDto"/> instances.</returns>
    public List<GenreDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
