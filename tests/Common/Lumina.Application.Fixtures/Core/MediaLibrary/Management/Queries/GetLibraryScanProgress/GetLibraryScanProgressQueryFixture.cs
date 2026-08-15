#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.MediaLibrary.Management.Queries.GetLibraryScanProgress;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.MediaLibrary.Management.Queries.GetLibraryScanProgress;

/// <summary>
/// Fixture class for the <see cref="GetLibraryScanProgressQuery"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryScanProgressQueryFixture
{
    /// <summary>
    /// Creates a random valid query to get the progress of a media library scan.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the media library whose scan progress is requested.</param>
    /// <param name="scanId">Optional. The Id of the media library scan whose progress is requested.</param>
    /// <returns>The created query.</returns>
    public GetLibraryScanProgressQuery Create(Guid? libraryId = null, Guid? scanId = null)
    {
        return new Faker<GetLibraryScanProgressQuery>()
            .CustomInstantiator(f => new GetLibraryScanProgressQuery(
                libraryId ?? Guid.NewGuid(),
                scanId ?? Guid.NewGuid()))
            .Generate();
    }

    /// <summary>
    /// Creates a list of <see cref="GetLibraryScanProgressQuery"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetLibraryScanProgressQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
