#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Themes.Management.Queries.GetThemeSettings;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.Themes.Management.Queries.GetThemeSettings;

/// <summary>
/// Fixture class for the <see cref="GetThemeSettingsQuery"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetThemeSettingsQueryFixture
{
    /// <summary>
    /// Creates a <see cref="GetThemeSettingsQuery"/>.
    /// </summary>
    /// <returns>The created <see cref="GetThemeSettingsQuery"/>.</returns>
    public GetThemeSettingsQuery Create()
    {
        return new GetThemeSettingsQuery();
    }

    /// <summary>
    /// Creates a list of <see cref="GetThemeSettingsQuery"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetThemeSettingsQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
