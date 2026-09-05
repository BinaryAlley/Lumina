#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.Scheduling.Commands.StopScheduledJob;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.Scheduling.Commands.StopScheduledJob;

/// <summary>
/// Fixture class for the <see cref="StopScheduledJobCommand"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class StopScheduledJobCommandFixture
{
    /// <summary>
    /// Creates a random valid command to stop the execution cycle of a scheduled job.
    /// </summary>
    /// <param name="scheduledJobId">Optional. The Id of the scheduled job whose execution cycle is stopped.</param>
    /// <returns>The created command.</returns>
    public StopScheduledJobCommand Create(
        Guid? scheduledJobId = null)
    {
        return new Faker<StopScheduledJobCommand>()
            .CustomInstantiator(f => new StopScheduledJobCommand(scheduledJobId ?? Guid.NewGuid()))
            .Generate();
    }

    /// <summary>
    /// Creates a list of <see cref="StopScheduledJobCommand"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<StopScheduledJobCommand> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
