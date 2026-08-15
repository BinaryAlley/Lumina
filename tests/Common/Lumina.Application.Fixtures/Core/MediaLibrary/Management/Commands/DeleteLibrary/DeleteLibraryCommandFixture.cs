#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.MediaLibrary.Management.Commands.DeleteLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.MediaLibrary.Management.Commands.DeleteLibrary;

/// <summary>
/// Fixture class for the <see cref="DeleteLibraryCommand"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteLibraryCommandFixture
{
    /// <summary>
    /// Creates a random valid command to delete a media library.
    /// </summary>
    /// <param name="id">Optional. The Id of the media library to delete.</param>
    /// <returns>The created command.</returns>
    public DeleteLibraryCommand Create(Guid? id = null)
    {
        return new Faker<DeleteLibraryCommand>()
            .CustomInstantiator(f => new DeleteLibraryCommand(
                id ?? Guid.NewGuid()))
            .Generate();
    }

    /// <summary>
    /// Creates a list of <see cref="DeleteLibraryCommand"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<DeleteLibraryCommand> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
