#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.Common;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DataAccess.Entities.Common;

/// <summary>
/// Fixture class for the <see cref="TagEntity"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class TagEntityFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="TagEntity"/>.
    /// </summary>
    /// <param name="name">Optional. The name of the tag.</param>
    /// <param name="includeName">Whether the name should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="TagEntity"/>.</returns>
    public TagEntity Create(
        string? name = null, 
        bool includeName = true)
    {
        return new TagEntity(includeName ? name ?? _faker.Lorem.Word() : null);
    }

    /// <summary>
    /// Creates a list of <see cref="TagEntity"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<TagEntity> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
