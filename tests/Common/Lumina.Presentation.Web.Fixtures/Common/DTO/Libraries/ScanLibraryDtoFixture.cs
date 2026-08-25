#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Libraries;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.Libraries;

/// <summary>
/// Fixture class for generating <see cref="ScanLibraryDto"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class ScanLibraryDtoFixture
{
    /// <summary>
    /// Creates a new <see cref="ScanLibraryDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="scanId">Optional Id of the media library scan.</param>
    /// <param name="libraryId">Optional Id of the scanned media library.</param>
    /// <returns>A configured <see cref="ScanLibraryDto"/> instance.</returns>
    public ScanLibraryDto Create(
        Guid? scanId = null, 
        Guid? libraryId = null)
    {
        return new ScanLibraryDto
        {
            ScanId = scanId ?? Guid.NewGuid(),
            LibraryId = libraryId ?? Guid.NewGuid()
        };
    }

    /// <summary>
    /// Creates multiple <see cref="ScanLibraryDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="ScanLibraryDto"/> instances.</returns>
    public List<ScanLibraryDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
