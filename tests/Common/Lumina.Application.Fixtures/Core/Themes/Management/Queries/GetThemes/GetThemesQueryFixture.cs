#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Themes.Management.Queries.GetThemes;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.Themes.Management.Queries.GetThemes;

/// <summary>
/// Fixture class for the <see cref="GetThemesQuery"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetThemesQueryFixture
{
    /// <summary>
    /// Creates a <see cref="GetThemesQuery"/>.
    /// </summary>
    /// <returns>The created <see cref="GetThemesQuery"/>.</returns>
    public GetThemesQuery Create()
    {
        return new GetThemesQuery();
    }

    /// <summary>
    /// Creates a list of <see cref="GetThemesQuery"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetThemesQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
