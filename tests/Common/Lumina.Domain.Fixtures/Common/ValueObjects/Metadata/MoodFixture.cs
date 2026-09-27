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
/// Fixture class for the <see cref="Mood"/> domain value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class MoodFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="Mood"/>.
    /// </summary>
    /// <param name="name">Optional. The name of the mood. If not provided, a random name is generated.</param>
    /// <returns>The created <see cref="Mood"/>.</returns>
    public Mood Create(
        string? name = null)
    {
        Result<Mood> moodResult = Mood.Create(name ?? _faker.Lorem.Word());

        if (moodResult.IsFailure)
            throw new InvalidOperationException("Failed to create Mood: " + string.Join(", ", moodResult.Errors));
        return moodResult.Value;
    }

    /// <summary>
    /// Creates multiple <see cref="Mood"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="Mood"/> instances.</returns>
    public List<Mood> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
