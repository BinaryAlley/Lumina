#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.MediaLibrary.Management.Commands.ScanLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.MediaLibrary.Management.Commands.ScanLibrary;

/// <summary>
/// Fixture class for the <see cref="ScanLibraryCommand"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ScanLibraryCommandFixture
{
    /// <summary>
    /// Creates a random valid command to scan a media library.
    /// </summary>
    /// <param name="id">Optional. The Id of the media library to scan.</param>
    /// <returns>The created command.</returns>
    public ScanLibraryCommand Create(Guid? id = null)
    {
        return new Faker<ScanLibraryCommand>()
            .CustomInstantiator(f => new ScanLibraryCommand(
                id ?? Guid.NewGuid()))
            .Generate();
    }

    /// <summary>
    /// Creates a list of <see cref="ScanLibraryCommand"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<ScanLibraryCommand> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
