#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.MediaConsumption.Commands.DeleteConsumptionRecord;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.MediaConsumption.Commands.DeleteConsumptionRecord;

/// <summary>
/// Fixture class for the <see cref="DeleteConsumptionRecordCommand"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteConsumptionRecordCommandFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="DeleteConsumptionRecordCommand"/>.
    /// </summary>
    /// <param name="consumptionRecordId">Optional. The Id of the consumption record to delete, taken from the route. Defaults to a random Id.</param>
    /// <param name="targetUserId">Optional. The Id of the user the record must belong to. Defaults to a random user Id.</param>
    /// <param name="includeConsumptionRecordId">Whether the consumption record Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeTargetUserId">Whether the target user Id should be included, or forced to <see langword="null"/> so that the current user is targeted.</param>
    /// <returns>The created <see cref="DeleteConsumptionRecordCommand"/>.</returns>
    public DeleteConsumptionRecordCommand Create(
        string? consumptionRecordId = null,
        string? targetUserId = null,
        bool includeConsumptionRecordId = true,
        bool includeTargetUserId = true)
    {
        string? resolvedConsumptionRecordId = includeConsumptionRecordId ? consumptionRecordId ?? _faker.Random.Guid().ToString() : null;
        string? resolvedTargetUserId = includeTargetUserId ? targetUserId ?? _faker.Random.Guid().ToString() : null;

        return new DeleteConsumptionRecordCommand(resolvedConsumptionRecordId, resolvedTargetUserId);
    }

    /// <summary>
    /// Creates a list of <see cref="DeleteConsumptionRecordCommand"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="DeleteConsumptionRecordCommand"/> instances.</returns>
    public List<DeleteConsumptionRecordCommand> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
