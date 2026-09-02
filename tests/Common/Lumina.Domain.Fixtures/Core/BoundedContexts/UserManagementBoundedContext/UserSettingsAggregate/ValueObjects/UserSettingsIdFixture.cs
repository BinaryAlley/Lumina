#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserSettingsAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.UserManagementBoundedContext.UserSettingsAggregate.ValueObjects;

/// <summary>
/// Fixture class for the <see cref="UserSettingsId"/> value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class UserSettingsIdFixture
{
    /// <summary>
    /// Creates a random valid <see cref="UserSettingsId"/>.
    /// </summary>
    /// <param name="value">Optional. The raw value of the user settings Id.</param>
    /// <returns>The created <see cref="UserSettingsId"/>.</returns>
    public UserSettingsId Create(Guid? value = null)
    {
        return UserSettingsId.Create(value ?? Guid.NewGuid());
    }

    /// <summary>
    /// Creates a list of <see cref="UserSettingsId"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="UserSettingsId"/> instances.</returns>
    public List<UserSettingsId> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
