#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;

/// <summary>
/// Fixture class for the <see cref="MusicBrainzId"/> domain value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicBrainzIdFixture
{
    /// <summary>
    /// Creates a random valid <see cref="MusicBrainzId"/>.
    /// </summary>
    /// <param name="value">Optional. The value used to create the <see cref="MusicBrainzId"/>. If not provided, a random value is generated.</param>
    /// <returns>The created <see cref="MusicBrainzId"/>.</returns>
    public MusicBrainzId Create(
        Guid? value = null)
    {
        return MusicBrainzId.Create(value ?? Guid.NewGuid());
    }

    /// <summary>
    /// Creates multiple <see cref="MusicBrainzId"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MusicBrainzId"/> instances.</returns>
    public List<MusicBrainzId> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
