#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.Scheduling.Commands.FireScheduledJob;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.Scheduling.Commands.FireScheduledJob;

/// <summary>
/// Fixture class for the <see cref="FireScheduledJobCommand"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class FireScheduledJobCommandFixture
{
    /// <summary>
    /// Creates a random valid command to fire the task of a scheduled job once.
    /// </summary>
    /// <param name="scheduledJobId">Optional. The Id of the scheduled job whose task is fired.</param>
    /// <returns>The created command.</returns>
    public FireScheduledJobCommand Create(
        Guid? scheduledJobId = null)
    {
        return new Faker<FireScheduledJobCommand>()
            .CustomInstantiator(f => new FireScheduledJobCommand(scheduledJobId ?? Guid.NewGuid()))
            .Generate();
    }

    /// <summary>
    /// Creates a list of <see cref="FireScheduledJobCommand"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<FireScheduledJobCommand> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
