#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Fixture class for generating <see cref="GetBookRequest"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetBookRequestFixture
{
    /// <summary>
    /// Creates a new <see cref="GetBookRequest"/> instance.
    /// </summary>
    /// <param name="id">Optional unique identifier of the book to retrieve.</param>
    /// <returns>A configured <see cref="GetBookRequest"/> instance.</returns>
    public GetBookRequest Create(
        Guid? id = null)
    {
        return new GetBookRequest(id?.ToString() ?? Guid.NewGuid().ToString());
    }

    /// <summary>
    /// Creates a list of <see cref="GetBookRequest"/> instances.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="GetBookRequest"/> instances.</returns>
    public List<GetBookRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
