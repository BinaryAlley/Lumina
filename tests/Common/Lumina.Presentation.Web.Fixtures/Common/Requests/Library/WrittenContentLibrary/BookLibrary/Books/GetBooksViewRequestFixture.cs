#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Fixture class for generating <see cref="GetBooksViewRequest"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetBooksViewRequestFixture
{
    /// <summary>
    /// Creates a new <see cref="GetBooksViewRequest"/> instance.
    /// </summary>
    /// <param name="libraryId">Optional unique identifier of the media library whose books are browsed.</param>
    /// <returns>A configured <see cref="GetBooksViewRequest"/> instance.</returns>
    public GetBooksViewRequest Create(
        Guid? libraryId = null)
    {
        return new GetBooksViewRequest(
            LibraryId: libraryId
        );
    }

    /// <summary>
    /// Creates multiple <see cref="GetBooksViewRequest"/> instances.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="GetBooksViewRequest"/> instances.</returns>
    public List<GetBooksViewRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
