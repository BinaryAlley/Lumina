#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserSettingsAggregate;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.UserManagementBoundedContext.UserSettingsAggregate;

/// <summary>
/// Fixture class for the <see cref="UserSettings"/> domain aggregate.
/// </summary>
[ExcludeFromCodeCoverage]
public class UserSettingsFixture
{
    /// <summary>
    /// Creates a random valid <see cref="UserSettings"/> domain aggregate.
    /// </summary>
    /// <param name="userId">Optional. The Id of the user that owns the settings.</param>
    /// <param name="isPaginationEnabled">Whether pagination is enabled for the user.</param>
    /// <param name="itemsPerPage">The number of library items displayed per page when pagination is enabled.</param>
    /// <param name="ignoreThePrefixForAlphaPicker">Whether the "The" prefix of library item titles is ignored by the alpha picker.</param>
    /// <returns>The created <see cref="UserSettings"/>.</returns>
    public UserSettings Create(
        Guid? userId = null,
        bool isPaginationEnabled = true,
        int itemsPerPage = 48,
        bool ignoreThePrefixForAlphaPicker = false)
    {
        Result<UserSettings> settings = UserSettings.Create(
            userId is null ? UserId.CreateUnique() : UserId.Create(userId.Value),
            isPaginationEnabled,
            itemsPerPage,
            ignoreThePrefixForAlphaPicker);
        return settings.Value;
    }

    /// <summary>
    /// Creates multiple <see cref="UserSettings"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="UserSettings"/> instances.</returns>
    public List<UserSettings> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
