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
/// Fixture class for the <see cref="ScanLibraryRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ScanLibraryRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="ScanLibraryRequest"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the media library to scan.</param>
    /// <returns>The created <see cref="ScanLibraryRequest"/>.</returns>
    public ScanLibraryRequest Create(Guid? id = null)
    {
        return new ScanLibraryRequest(
            id ?? _faker.Random.Guid()
        );
    }

    /// <summary>
    /// Creates a list of <see cref="ScanLibraryRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<ScanLibraryRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
