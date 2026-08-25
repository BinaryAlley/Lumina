#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.Management.Queries.GetLibraries;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.MediaLibrary.Management.Queries.GetLibraries;

/// <summary>
/// Fixture class for the <see cref="GetLibrariesQuery"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibrariesQueryFixture
{
    /// <summary>
    /// Creates a <see cref="GetLibrariesQuery"/>.
    /// </summary>
    /// <returns>The created <see cref="GetLibrariesQuery"/>.</returns>
    public GetLibrariesQuery Create()
    {
        return new GetLibrariesQuery();
    }

    /// <summary>
    /// Creates a list of <see cref="GetLibrariesQuery"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetLibrariesQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
