#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Requests.Authorization;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.Authorization;

/// <summary>
/// Fixture class for the <see cref="GetAuthorizationRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetAuthorizationRequestFixture
{
    /// <summary>
    /// Creates a <see cref="GetAuthorizationRequest"/> with default or specified values.
    /// </summary>
    /// <param name="userId">Optional. The user Id to use.</param>
    /// <returns>The created <see cref="GetAuthorizationRequest"/>.</returns>
    public GetAuthorizationRequest Create(Guid? userId = null)
    {
        return new GetAuthorizationRequest(
            UserId: userId ?? Guid.NewGuid()
        );
    }

    /// <summary>
    /// Creates a list of <see cref="GetAuthorizationRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetAuthorizationRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
