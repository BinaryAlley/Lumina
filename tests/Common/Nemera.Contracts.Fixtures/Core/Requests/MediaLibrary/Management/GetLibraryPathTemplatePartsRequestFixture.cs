#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Requests.MediaLibrary.Management;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.Management;

/// <summary>
/// Fixture class for the <see cref="GetLibraryPathTemplatePartsRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryPathTemplatePartsRequestFixture
{
    /// <summary>
    /// Creates a random valid <see cref="GetLibraryPathTemplatePartsRequest"/>.
    /// </summary>
    /// <param name="libraryType">Optional. The media library type whose available path parts are retrieved.</param>
    /// <returns>The created <see cref="GetLibraryPathTemplatePartsRequest"/>.</returns>
    public GetLibraryPathTemplatePartsRequest Create(string? libraryType = null)
    {
        return new GetLibraryPathTemplatePartsRequest(libraryType ?? "Music");
    }

    /// <summary>
    /// Creates a list of <see cref="GetLibraryPathTemplatePartsRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetLibraryPathTemplatePartsRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
