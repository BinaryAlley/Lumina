#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.UsersManagement.Settings.Queries.GetUserSettings;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.UsersManagement.Settings.Queries.GetUserSettings;

/// <summary>
/// Fixture class for the <see cref="GetUserSettingsQuery"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetUserSettingsQueryFixture
{
    /// <summary>
    /// Creates a random valid query to get the settings of the current user.
    /// </summary>
    /// <returns>The created query.</returns>
    public GetUserSettingsQuery Create()
    {
        return new GetUserSettingsQuery();
    }

    /// <summary>
    /// Creates a list of <see cref="GetUserSettingsQuery"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetUserSettingsQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
