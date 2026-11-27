#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.SchedulingBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;

/// <summary>
/// Fixture class for the <see cref="UserId"/> value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class UserIdFixture
{
    /// <summary>
    /// Creates a random valid <see cref="UserId"/>.
    /// </summary>
    /// <param name="value">Optional. The raw value of the user Id.</param>
    /// <returns>The created <see cref="UserId"/>.</returns>
    public UserId Create(
        Guid? value = null)
    {
        return UserId.Create(value ?? Guid.NewGuid());
    }

    /// <summary>
    /// Creates a list of <see cref="UserId"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="UserId"/> instances.</returns>
    public List<UserId> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
