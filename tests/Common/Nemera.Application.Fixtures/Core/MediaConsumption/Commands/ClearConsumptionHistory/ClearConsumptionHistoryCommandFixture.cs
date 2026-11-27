#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.MediaConsumption.Commands.ClearConsumptionHistory;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.MediaConsumption.Commands.ClearConsumptionHistory;

/// <summary>
/// Fixture class for the <see cref="ClearConsumptionHistoryCommand"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ClearConsumptionHistoryCommandFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="ClearConsumptionHistoryCommand"/>.
    /// </summary>
    /// <param name="targetUserId">Optional. The Id of the user whose consumption history is cleared. Defaults to a random user Id.</param>
    /// <param name="includeTargetUserId">Whether the target user Id should be included, or forced to <see langword="null"/> so that the current user is targeted.</param>
    /// <returns>The created <see cref="ClearConsumptionHistoryCommand"/>.</returns>
    public ClearConsumptionHistoryCommand Create(
        string? targetUserId = null,
        bool includeTargetUserId = true)
    {
        string? resolvedTargetUserId = includeTargetUserId ? targetUserId ?? _faker.Random.Guid().ToString() : null;

        return new ClearConsumptionHistoryCommand(resolvedTargetUserId);
    }

    /// <summary>
    /// Creates a list of <see cref="ClearConsumptionHistoryCommand"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="ClearConsumptionHistoryCommand"/> instances.</returns>
    public List<ClearConsumptionHistoryCommand> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
