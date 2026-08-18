#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;

/// <summary>
/// Fixture class for the <see cref="LibraryScanEntity"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryScanEntityFixture
{
    private readonly LibraryEntityFixture _libraryEntityFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="LibraryScanEntity"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the media library scan.</param>
    /// <param name="libraryId">Optional. The Id of the media library that is scanned.</param>
    /// <param name="userId">Optional. The Id of the user that initiated the media library scan.</param>
    /// <param name="status">The status of the media library scan.</param>
    /// <param name="libraryEntity">Optional. The media library that is scanned.</param>
    /// <param name="createdOnUtc">Optional. The time and date when the entity was added.</param>
    /// <returns>The created media library scan entity.</returns>
    public LibraryScanEntity Create(
        Guid? id = null,
        Guid? libraryId = null,
        Guid? userId = null,
        LibraryScanJobStatus? status = null,
        LibraryEntity? libraryEntity = null,
        DateTime? createdOnUtc = null)
    {
        Guid resolvedLibraryId = libraryId ?? Guid.NewGuid();
        Guid resolvedUserId = userId ?? Guid.NewGuid();
        LibraryEntity resolvedLibrary = libraryEntity ?? _libraryEntityFixture.Create(id: resolvedLibraryId, userId: resolvedUserId);

        return new Faker<LibraryScanEntity>()
            .CustomInstantiator(f => new LibraryScanEntity
            {
                Id = id ?? Guid.NewGuid(),
                LibraryId = resolvedLibraryId,
                UserId = resolvedUserId,
                Status = status ?? LibraryScanJobStatus.Running,
                Library = resolvedLibrary,
                User = null!,
                LibraryScanResults = [],
                CreatedOnUtc = createdOnUtc ?? f.Date.Past(),
                CreatedBy = resolvedUserId,
                UpdatedOnUtc = null,
                UpdatedBy = null
            })
            .Generate();
    }

    /// <summary>
    /// Creates a list of <see cref="LibraryScanEntity"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="LibraryScanEntity"/> instances.</returns>
    public List<LibraryScanEntity> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
