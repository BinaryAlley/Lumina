#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.ConsumptionActivity;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.Infrastructure.Authorization.Policies.ConsumptionActivity;

/// <summary>
/// Fixture class for the <see cref="ConsumptionActivityAccessPolicyContext"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ConsumptionActivityAccessPolicyContextFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="ConsumptionActivityAccessPolicyContext"/> instance with randomized test data.
    /// </summary>
    /// <param name="targetUserId">Optional. The Id of the user whose consumption activity is being accessed.</param>
    /// <returns>The created <see cref="ConsumptionActivityAccessPolicyContext"/> instance.</returns>
    public ConsumptionActivityAccessPolicyContext Create(Guid? targetUserId = null)
    {
        return new ConsumptionActivityAccessPolicyContext(targetUserId ?? _faker.Random.Guid());
    }

    /// <summary>
    /// Creates a list of <see cref="ConsumptionActivityAccessPolicyContext"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="ConsumptionActivityAccessPolicyContext"/> instances.</returns>
    public List<ConsumptionActivityAccessPolicyContext> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
