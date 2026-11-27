#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Themes.Management.Queries.GetCurrentTheme;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.Themes.Management.Queries.GetCurrentTheme;

/// <summary>
/// Fixture class for the <see cref="GetCurrentThemeQuery"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetCurrentThemeQueryFixture
{
    /// <summary>
    /// Creates a <see cref="GetCurrentThemeQuery"/>.
    /// </summary>
    /// <returns>The created <see cref="GetCurrentThemeQuery"/>.</returns>
    public GetCurrentThemeQuery Create()
    {
        return new GetCurrentThemeQuery();
    }

    /// <summary>
    /// Creates a list of <see cref="GetCurrentThemeQuery"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetCurrentThemeQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
