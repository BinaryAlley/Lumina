#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Requests.Library.Management;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.Library.Management;

/// <summary>
/// Fixture class for the <see cref="GetLibraryTypeBookReadersRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryTypeBookReadersRequestFixture
{
    /// <summary>
    /// Creates a random valid <see cref="GetLibraryTypeBookReadersRequest"/>.
    /// </summary>
    /// <param name="libraryType">Optional. The media library type whose available book readers are retrieved.</param>
    /// <returns>The created <see cref="GetLibraryTypeBookReadersRequest"/>.</returns>
    public GetLibraryTypeBookReadersRequest Create(
        string? libraryType = null)
    {
        return new GetLibraryTypeBookReadersRequest(libraryType ?? "Book");
    }

    /// <summary>
    /// Creates a list of <see cref="GetLibraryTypeBookReadersRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetLibraryTypeBookReadersRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
