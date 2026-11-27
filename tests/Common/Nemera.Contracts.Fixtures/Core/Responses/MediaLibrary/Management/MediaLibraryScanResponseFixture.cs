#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Responses.MediaLibrary.Management;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Responses.MediaLibrary.Management;

/// <summary>
/// Fixture class for the <see cref="MediaLibraryScanResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaLibraryScanResponseFixture
{
    /// <summary>
    /// Creates a random valid <see cref="MediaLibraryScanResponse"/>.
    /// </summary>
    /// <param name="scanId">Optional. The Id of the media library scan.</param>
    /// <param name="libraryId">Optional. The Id of the media library the scan belongs to.</param>
    /// <returns>The created <see cref="MediaLibraryScanResponse"/>.</returns>
    public MediaLibraryScanResponse Create(
        Guid? scanId = null, 
        Guid? libraryId = null)
    {
        return new MediaLibraryScanResponse(scanId ?? Guid.NewGuid(), libraryId ?? Guid.NewGuid());
    }

    /// <summary>
    /// Creates a list of <see cref="MediaLibraryScanResponse"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<MediaLibraryScanResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
