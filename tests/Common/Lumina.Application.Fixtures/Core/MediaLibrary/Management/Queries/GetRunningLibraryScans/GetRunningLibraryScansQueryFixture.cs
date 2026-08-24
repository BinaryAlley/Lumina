#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.Management.Queries.GetRunningLibraryScans;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.MediaLibrary.Management.Queries.GetRunningLibraryScans;

/// <summary>
/// Fixture class for the <see cref="GetRunningLibraryScansQuery"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetRunningLibraryScansQueryFixture
{
    /// <summary>
    /// Creates a <see cref="GetRunningLibraryScansQuery"/>.
    /// </summary>
    /// <returns>The created <see cref="GetRunningLibraryScansQuery"/>.</returns>
    public GetRunningLibraryScansQuery Create()
    {
        return new GetRunningLibraryScansQuery();
    }

    /// <summary>
    /// Creates a list of <see cref="GetRunningLibraryScansQuery"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetRunningLibraryScansQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
