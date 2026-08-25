#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Plugins.Queries.GetPlugins;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.Plugins.Queries.GetPlugins;

/// <summary>
/// Fixture class for the <see cref="GetPluginsQuery"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPluginsQueryFixture
{
    /// <summary>
    /// Creates a <see cref="GetPluginsQuery"/>.
    /// </summary>
    /// <returns>The created <see cref="GetPluginsQuery"/>.</returns>
    public GetPluginsQuery Create()
    {
        return new GetPluginsQuery();
    }

    /// <summary>
    /// Creates a list of <see cref="GetPluginsQuery"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetPluginsQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
