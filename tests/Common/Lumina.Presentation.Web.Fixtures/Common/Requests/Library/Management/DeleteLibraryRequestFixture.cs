#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.Requests.Library.Management;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.Library.Management;

/// <summary>
/// Fixture class for generating <see cref="DeleteLibraryRequest"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteLibraryRequestFixture
{
    /// <summary>
    /// Creates a new <see cref="DeleteLibraryRequest"/> instance with randomized test data.
    /// </summary>
    /// <param name="id">Optional identifier of the media library.</param>
    /// <returns>A configured <see cref="DeleteLibraryRequest"/> instance.</returns>
    public DeleteLibraryRequest Create(Guid? id = null)
    {
        return new DeleteLibraryRequest(
            Id: id ?? Guid.NewGuid()
        );
    }

    /// <summary>
    /// Creates multiple <see cref="DeleteLibraryRequest"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="DeleteLibraryRequest"/> instances.</returns>
    public List<DeleteLibraryRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
