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
/// Fixture class for the <see cref="DeleteLibraryRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteLibraryRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="DeleteLibraryRequest"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the media library to delete.</param>
    /// <returns>The created <see cref="DeleteLibraryRequest"/>.</returns>
    public DeleteLibraryRequest Create(Guid? id = null)
    {
        return new DeleteLibraryRequest(
            id ?? _faker.Random.Guid()
        );
    }

    /// <summary>
    /// Creates a list of <see cref="DeleteLibraryRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<DeleteLibraryRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
