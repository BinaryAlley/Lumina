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
/// Fixture class for the <see cref="Tag"/> domain value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class TagFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="Tag"/>.
    /// </summary>
    /// <param name="name">Optional. The name of the tag. If not provided, a random name is generated.</param>
    /// <returns>The created <see cref="Tag"/>.</returns>
    public Tag Create(string? name = null)
    {
        Result<Tag> tagResult = Tag.Create(name ?? _faker.Lorem.Word());

        if (tagResult.IsFailure)
            throw new InvalidOperationException("Failed to create Tag: " + string.Join(", ", tagResult.Errors));
        return tagResult.Value;
    }

    /// <summary>
    /// Creates multiple <see cref="Tag"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="Tag"/> instances.</returns>
    public List<Tag> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
