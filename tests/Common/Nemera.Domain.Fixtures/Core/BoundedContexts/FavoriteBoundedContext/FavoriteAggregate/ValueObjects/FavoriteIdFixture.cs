#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.FavoriteBoundedContext.FavoriteAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.FavoriteBoundedContext.FavoriteAggregate.ValueObjects;

/// <summary>
/// Fixture class for the <see cref="FavoriteId"/> value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class FavoriteIdFixture
{
    /// <summary>
    /// Creates a random valid <see cref="FavoriteId"/>.
    /// </summary>
    /// <param name="value">Optional. The raw value of the favorite Id.</param>
    /// <returns>The created <see cref="FavoriteId"/>.</returns>
    public FavoriteId Create(
        Guid? value = null)
    {
        return FavoriteId.Create(value ?? Guid.NewGuid());
    }

    /// <summary>
    /// Creates a list of <see cref="FavoriteId"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="FavoriteId"/> instances.</returns>
    public List<FavoriteId> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
