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
/// Fixture class for the <see cref="StartScheduledJobRequest"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class StartScheduledJobRequestFixture
{
    /// <summary>
    /// Creates a random valid request to start the execution cycle of a scheduled job.
    /// </summary>
    /// <param name="scheduledJobId">Optional. The Id of the scheduled job whose execution cycle is started.</param>
    /// <returns>The created request.</returns>
    public StartScheduledJobRequest Create(
        Guid? scheduledJobId = null)
    {
        return new Faker<StartScheduledJobRequest>()
            .CustomInstantiator(f => new StartScheduledJobRequest(scheduledJobId ?? Guid.NewGuid()))
            .Generate();
    }

    /// <summary>
    /// Creates a list of <see cref="StartScheduledJobRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<StartScheduledJobRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
