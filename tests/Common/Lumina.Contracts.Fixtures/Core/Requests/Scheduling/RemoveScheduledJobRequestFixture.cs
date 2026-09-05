#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Requests.Scheduling;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.Scheduling;

/// <summary>
/// Fixture class for the <see cref="RemoveScheduledJobRequest"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RemoveScheduledJobRequestFixture
{
    /// <summary>
    /// Creates a random valid request to remove a scheduled job.
    /// </summary>
    /// <param name="scheduledJobId">Optional. The Id of the scheduled job to remove.</param>
    /// <returns>The created request.</returns>
    public RemoveScheduledJobRequest Create(
        Guid? scheduledJobId = null)
    {
        return new Faker<RemoveScheduledJobRequest>()
            .CustomInstantiator(f => new RemoveScheduledJobRequest(scheduledJobId ?? Guid.NewGuid()))
            .Generate();
    }

    /// <summary>
    /// Creates a list of <see cref="RemoveScheduledJobRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<RemoveScheduledJobRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
