#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.Scheduling.Commands.StartScheduledJob;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.Scheduling.Commands.StartScheduledJob;

/// <summary>
/// Fixture class for the <see cref="StartScheduledJobCommand"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class StartScheduledJobCommandFixture
{
    /// <summary>
    /// Creates a random valid command to start the execution cycle of a scheduled job.
    /// </summary>
    /// <param name="scheduledJobId">Optional. The Id of the scheduled job whose execution cycle is started.</param>
    /// <returns>The created command.</returns>
    public StartScheduledJobCommand Create(
        Guid? scheduledJobId = null)
    {
        return new Faker<StartScheduledJobCommand>()
            .CustomInstantiator(f => new StartScheduledJobCommand(scheduledJobId ?? Guid.NewGuid()))
            .Generate();
    }

    /// <summary>
    /// Creates a list of <see cref="StartScheduledJobCommand"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<StartScheduledJobCommand> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
