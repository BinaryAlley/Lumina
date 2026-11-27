#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.Management.Commands.CancelLibrariesScan;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.MediaLibrary.Management.Commands.CancelLibrariesScan;

/// <summary>
/// Fixture class for the <see cref="CancelLibrariesScanCommand"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CancelLibrariesScanCommandFixture
{
    /// <summary>
    /// Creates a <see cref="CancelLibrariesScanCommand"/>.
    /// </summary>
    /// <returns>The created <see cref="CancelLibrariesScanCommand"/>.</returns>
    public CancelLibrariesScanCommand Create()
    {
        return new CancelLibrariesScanCommand();
    }

    /// <summary>
    /// Creates a list of <see cref="CancelLibrariesScanCommand"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<CancelLibrariesScanCommand> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
