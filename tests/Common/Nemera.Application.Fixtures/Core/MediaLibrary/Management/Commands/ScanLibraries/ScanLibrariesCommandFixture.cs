#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.Management.Commands.ScanLibraries;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.MediaLibrary.Management.Commands.ScanLibraries;

/// <summary>
/// Fixture class for the <see cref="ScanLibrariesCommand"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ScanLibrariesCommandFixture
{
    /// <summary>
    /// Creates a <see cref="ScanLibrariesCommand"/>.
    /// </summary>
    /// <returns>The created <see cref="ScanLibrariesCommand"/>.</returns>
    public ScanLibrariesCommand Create()
    {
        return new ScanLibrariesCommand();
    }

    /// <summary>
    /// Creates a list of <see cref="ScanLibrariesCommand"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<ScanLibrariesCommand> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
