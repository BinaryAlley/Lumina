#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.MediaLibrary.Management.Commands.CancelLibraryScan;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.MediaLibrary.Management.Commands.CancelLibraryScan;

/// <summary>
/// Fixture class for the <see cref="CancelLibraryScanCommand"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CancelLibraryScanCommandFixture
{
    /// <summary>
    /// Creates a random valid command to cancel a media library scan.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the media library whose scan is cancelled.</param>
    /// <param name="scanId">Optional. The Id of the scan to cancel.</param>
    /// <returns>The created command.</returns>
    public CancelLibraryScanCommand Create(Guid? libraryId = null, Guid? scanId = null)
    {
        return new Faker<CancelLibraryScanCommand>()
            .CustomInstantiator(f => new CancelLibraryScanCommand(
                libraryId ?? Guid.NewGuid(),
                scanId ?? Guid.NewGuid()))
            .Generate();
    }

    /// <summary>
    /// Creates a list of <see cref="CancelLibraryScanCommand"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<CancelLibraryScanCommand> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
