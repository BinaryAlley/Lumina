#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Requests.Authorization;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.Authorization;

/// <summary>
/// Fixture class for the <see cref="GetUserPermissionsRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetUserPermissionsRequestFixture
{
    /// <summary>
    /// Creates a random valid <see cref="GetUserPermissionsRequest"/>.
    /// </summary>
    /// <param name="userId">Optional. The Id of the user whose permissions are retrieved.</param>
    /// <returns>The created <see cref="GetUserPermissionsRequest"/>.</returns>
    public GetUserPermissionsRequest Create(
        Guid? userId = null)
    {
        return new GetUserPermissionsRequest(userId ?? Guid.NewGuid());
    }

    /// <summary>
    /// Creates a list of <see cref="GetUserPermissionsRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetUserPermissionsRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
