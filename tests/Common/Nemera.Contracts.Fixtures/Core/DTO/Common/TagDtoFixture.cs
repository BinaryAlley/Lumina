#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.DTO.Common;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.DTO.Common;

/// <summary>
/// Fixture class for the <see cref="TagDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class TagDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="TagDto"/>.
    /// </summary>
    /// <param name="name">Optional. The name of the tag.</param>
    /// <param name="includeName">Whether the name should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="TagDto"/>.</returns>
    public TagDto Create(
        string? name = null, 
        bool includeName = true)
    {
        return new TagDto(includeName ? name ?? _faker.Lorem.Word() : null);
    }

    /// <summary>
    /// Creates a list of <see cref="TagDto"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<TagDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
