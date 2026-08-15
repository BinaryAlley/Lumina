#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.FileSystemManagement.Drives.Queries.GetDrives;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.FileSystemManagement.Drives.Queries.GetDrives;

/// <summary>
/// Fixture class for the <see cref="GetDrivesQuery"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetDrivesQueryFixture
{
    /// <summary>
    /// Creates a random valid query to get drives.
    /// </summary>
    /// <returns>The created query.</returns>
    public GetDrivesQuery Create()
    {
        return new GetDrivesQuery();
    }

    /// <summary>
    /// Creates a list of <see cref="GetDrivesQuery"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetDrivesQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
