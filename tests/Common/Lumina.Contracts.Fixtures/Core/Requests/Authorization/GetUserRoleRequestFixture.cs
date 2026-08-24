#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Requests.Authorization;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.Authorization;

/// <summary>
/// Fixture class for the <see cref="GetUserRoleRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetUserRoleRequestFixture
{
    /// <summary>
    /// Creates a random valid <see cref="GetUserRoleRequest"/>.
    /// </summary>
    /// <param name="userId">Optional. The Id of the user whose role is retrieved.</param>
    /// <returns>The created <see cref="GetUserRoleRequest"/>.</returns>
    public GetUserRoleRequest Create(
        Guid? userId = null)
    {
        return new GetUserRoleRequest(userId ?? Guid.NewGuid());
    }

    /// <summary>
    /// Creates a list of <see cref="GetUserRoleRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetUserRoleRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
