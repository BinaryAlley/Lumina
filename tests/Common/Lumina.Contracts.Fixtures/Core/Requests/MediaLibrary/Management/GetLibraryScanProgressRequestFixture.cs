#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Requests.MediaLibrary.Management;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.Management;

/// <summary>
/// Fixture class for the <see cref="GetLibraryScanProgressRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryScanProgressRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="GetLibraryScanProgressRequest"/>.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the media library whose scan progress is requested.</param>
    /// <param name="scanId">Optional. The Id of the media library scan whose progress is requested.</param>
    /// <returns>The created <see cref="GetLibraryScanProgressRequest"/>.</returns>
    public GetLibraryScanProgressRequest Create(
        Guid? libraryId = null,
        Guid? scanId = null)
    {
        return new GetLibraryScanProgressRequest(
            libraryId ?? _faker.Random.Guid(),
            scanId ?? _faker.Random.Guid()
        );
    }

    /// <summary>
    /// Creates a list of <see cref="GetLibraryScanProgressRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetLibraryScanProgressRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
