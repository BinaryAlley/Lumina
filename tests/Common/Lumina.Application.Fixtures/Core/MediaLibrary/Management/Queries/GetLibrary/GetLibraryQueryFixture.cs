#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.MediaLibrary.Management.Queries.GetLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.MediaLibrary.Management.Queries.GetLibrary;

/// <summary>
/// Fixture class for the <see cref="GetLibraryQuery"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryQueryFixture
{
    /// <summary>
    /// Creates a random valid query to get a media library.
    /// </summary>
    /// <param name="id">Optional. The Id of the media library to get.</param>
    /// <returns>The created query.</returns>
    public GetLibraryQuery Create(Guid? id = null)
    {
        return new Faker<GetLibraryQuery>()
            .CustomInstantiator(f => new GetLibraryQuery(
                id ?? Guid.NewGuid()))
            .Generate();
    }

    /// <summary>
    /// Creates a list of <see cref="GetLibraryQuery"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetLibraryQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
