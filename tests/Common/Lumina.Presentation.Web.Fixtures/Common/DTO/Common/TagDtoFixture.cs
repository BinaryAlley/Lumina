#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.Common;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.Common;

/// <summary>
/// Fixture class for generating <see cref="TagDto"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class TagDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="TagDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="name">Optional. The name of the tag.</param>
    /// <returns>A configured <see cref="TagDto"/> instance.</returns>
    public TagDto Create(string? name = null)
    {
        return new TagDto
        {
            Name = name ?? _faker.Lorem.Word()
        };
    }

    /// <summary>
    /// Creates multiple <see cref="TagDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="TagDto"/> instances.</returns>
    public List<TagDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
