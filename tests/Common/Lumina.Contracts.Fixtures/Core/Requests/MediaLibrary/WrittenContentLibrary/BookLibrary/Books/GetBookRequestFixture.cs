#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Fixture class for the <see cref="GetBookRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetBookRequestFixture
{
    /// <summary>
    /// Creates a random valid <see cref="GetBookRequest"/>.
    /// </summary>
    /// <param name="id">Optional. The unique identifier of the book to retrieve.</param>
    /// <returns>The created <see cref="GetBookRequest"/>.</returns>
    public GetBookRequest Create(
        string? id = null)
    {
        return new GetBookRequest(id ?? Guid.NewGuid().ToString());
    }

    /// <summary>
    /// Creates a list of <see cref="GetBookRequest"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="GetBookRequest"/> instances.</returns>
    public List<GetBookRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
