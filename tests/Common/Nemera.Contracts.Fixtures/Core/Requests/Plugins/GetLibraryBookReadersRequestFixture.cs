#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Requests.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.Plugins;

/// <summary>
/// Fixture class for the <see cref="GetLibraryBookReadersRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryBookReadersRequestFixture
{
    /// <summary>
    /// Creates a random valid <see cref="GetLibraryBookReadersRequest"/>.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the media library whose book readers are retrieved.</param>
    /// <returns>The created <see cref="GetLibraryBookReadersRequest"/>.</returns>
    public GetLibraryBookReadersRequest Create(
        Guid? libraryId = null)
    {
        return new GetLibraryBookReadersRequest(libraryId ?? Guid.NewGuid());
    }

    /// <summary>
    /// Creates a list of <see cref="GetLibraryBookReadersRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetLibraryBookReadersRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
