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
/// Fixture class for the <see cref="CancelLibraryScanRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class CancelLibraryScanRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="CancelLibraryScanRequest"/>.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the media library whose scan is cancelled.</param>
    /// <param name="scanId">Optional. The Id of the scan to cancel.</param>
    /// <returns>The created <see cref="CancelLibraryScanRequest"/>.</returns>
    public CancelLibraryScanRequest Create(
        Guid? libraryId = null,
        Guid? scanId = null)
    {
        return new CancelLibraryScanRequest(
            libraryId ?? _faker.Random.Guid(),
            scanId ?? _faker.Random.Guid()
        );
    }

    /// <summary>
    /// Creates a list of <see cref="CancelLibraryScanRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<CancelLibraryScanRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
