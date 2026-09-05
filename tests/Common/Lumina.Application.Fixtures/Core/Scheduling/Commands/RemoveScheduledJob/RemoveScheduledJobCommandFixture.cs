#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.Scheduling.Commands.RemoveScheduledJob;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.Scheduling.Commands.RemoveScheduledJob;

/// <summary>
/// Fixture class for the <see cref="RemoveScheduledJobCommand"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RemoveScheduledJobCommandFixture
{
    /// <summary>
    /// Creates a random valid command to remove a scheduled job.
    /// </summary>
    /// <param name="scheduledJobId">Optional. The Id of the scheduled job to remove.</param>
    /// <returns>The created command.</returns>
    public RemoveScheduledJobCommand Create(
        Guid? scheduledJobId = null)
    {
        return new Faker<RemoveScheduledJobCommand>()
            .CustomInstantiator(f => new RemoveScheduledJobCommand(scheduledJobId ?? Guid.NewGuid()))
            .Generate();
    }

    /// <summary>
    /// Creates a list of <see cref="RemoveScheduledJobCommand"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<RemoveScheduledJobCommand> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
