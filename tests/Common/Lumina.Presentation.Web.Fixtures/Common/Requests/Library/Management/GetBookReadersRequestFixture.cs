#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Requests.Library.Management;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.Library.Management;

/// <summary>
/// Fixture class for the <see cref="GetBookReadersRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetBookReadersRequestFixture
{
    /// <summary>
    /// Creates a random valid <see cref="GetBookReadersRequest"/>.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the media library whose book readers are retrieved.</param>
    /// <returns>The created <see cref="GetBookReadersRequest"/>.</returns>
    public GetBookReadersRequest Create(
        Guid? libraryId = null)
    {
        return new GetBookReadersRequest(libraryId ?? Guid.NewGuid());
    }

    /// <summary>
    /// Creates a list of <see cref="GetBookReadersRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetBookReadersRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
