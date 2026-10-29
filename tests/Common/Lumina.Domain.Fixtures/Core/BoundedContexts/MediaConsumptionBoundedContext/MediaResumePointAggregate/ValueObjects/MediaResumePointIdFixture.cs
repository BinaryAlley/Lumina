#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaResumePointAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaResumePointAggregate.ValueObjects;

/// <summary>
/// Fixture class for the <see cref="MediaResumePointId"/> value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaResumePointIdFixture
{
    /// <summary>
    /// Creates a random valid <see cref="MediaResumePointId"/>.
    /// </summary>
    /// <param name="value">Optional. The raw value of the resume point Id.</param>
    /// <returns>The created <see cref="MediaResumePointId"/>.</returns>
    public MediaResumePointId Create(
        Guid? value = null)
    {
        return MediaResumePointId.Create(value ?? Guid.NewGuid());
    }

    /// <summary>
    /// Creates a list of <see cref="MediaResumePointId"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MediaResumePointId"/> instances.</returns>
    public List<MediaResumePointId> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
