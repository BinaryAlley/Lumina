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
/// Fixture class for the <see cref="FireScheduledJobRequest"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class FireScheduledJobRequestFixture
{
    /// <summary>
    /// Creates a random valid request to fire the task of a scheduled job once.
    /// </summary>
    /// <param name="scheduledJobId">Optional. The Id of the scheduled job whose task is fired.</param>
    /// <returns>The created request.</returns>
    public FireScheduledJobRequest Create(
        Guid? scheduledJobId = null)
    {
        return new Faker<FireScheduledJobRequest>()
            .CustomInstantiator(f => new FireScheduledJobRequest(scheduledJobId ?? Guid.NewGuid()))
            .Generate();
    }

    /// <summary>
    /// Creates a list of <see cref="FireScheduledJobRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<FireScheduledJobRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
