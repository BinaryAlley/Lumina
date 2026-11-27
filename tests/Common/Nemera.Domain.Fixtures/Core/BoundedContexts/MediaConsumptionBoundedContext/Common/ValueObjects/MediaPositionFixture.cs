#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.Common.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.Common.ValueObjects;

/// <summary>
/// Fixture class for the <see cref="MediaPosition"/> value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaPositionFixture
{
    private const int MAXIMUM_OFFSET_MILLISECONDS = 7_200_000;

    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="MediaPosition"/>.
    /// </summary>
    /// <param name="kind">Optional. The kind of representation that the position uses. Defaults to a random valid kind.</param>
    /// <param name="timeOffset">Optional. The time offset of the position. Only used when <paramref name="kind"/> is <see cref="MediaPositionKind.TimeOffset"/>.</param>
    /// <param name="location">Optional. The location of the position. Only used when <paramref name="kind"/> is <see cref="MediaPositionKind.Location"/>.</param>
    /// <returns>The created <see cref="MediaPosition"/>.</returns>
    /// <remarks>
    /// The kind selects which representation is populated, so exactly one of <paramref name="timeOffset"/> and <paramref name="location"/> is used. A
    /// position always carries the representation of its own kind and can never carry both or neither, so no include flag is offered for them.
    /// There is no flag for an explicit empty <paramref name="location"/> either, because an empty string is already an explicit value.
    /// </remarks>
    public MediaPosition Create(
        MediaPositionKind? kind = null,
        TimeSpan? timeOffset = null,
        string? location = null)
    {
        MediaPositionKind resolvedKind = kind ?? _faker.Random.ArrayElement([MediaPositionKind.TimeOffset, MediaPositionKind.Location]);
        if (resolvedKind == MediaPositionKind.Location)
            return MediaPosition.CreateLocation(location ?? _faker.Lorem.Word());
        return MediaPosition.CreateTimeOffset(timeOffset ?? TimeSpan.FromMilliseconds(_faker.Random.Int(0, MAXIMUM_OFFSET_MILLISECONDS)));
    }

    /// <summary>
    /// Creates a list of <see cref="MediaPosition"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MediaPosition"/> instances.</returns>
    public List<MediaPosition> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
