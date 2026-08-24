#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.Management.Queries.GetEnabledLibraries;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.MediaLibrary.Management.Queries.GetEnabledLibraries;

/// <summary>
/// Fixture class for the <see cref="GetEnabledLibrariesQuery"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetEnabledLibrariesQueryFixture
{
    /// <summary>
    /// Creates a <see cref="GetEnabledLibrariesQuery"/>.
    /// </summary>
    /// <returns>The created <see cref="GetEnabledLibrariesQuery"/>.</returns>
    public GetEnabledLibrariesQuery Create()
    {
        return new GetEnabledLibrariesQuery();
    }

    /// <summary>
    /// Creates a list of <see cref="GetEnabledLibrariesQuery"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetEnabledLibrariesQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
