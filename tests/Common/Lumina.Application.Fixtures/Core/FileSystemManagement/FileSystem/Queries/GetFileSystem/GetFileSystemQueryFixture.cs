#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.FileSystemManagement.FileSystem.Queries.GetFileSystem;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.FileSystemManagement.FileSystem.Queries.GetFileSystem;

/// <summary>
/// Fixture class for the <see cref="GetFileSystemQuery"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetFileSystemQueryFixture
{
    /// <summary>
    /// Creates a random valid query to get the file system.
    /// </summary>
    /// <returns>The created query.</returns>
    public GetFileSystemQuery Create()
    {
        return new GetFileSystemQuery();
    }

    /// <summary>
    /// Creates a list of <see cref="GetFileSystemQuery"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetFileSystemQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
