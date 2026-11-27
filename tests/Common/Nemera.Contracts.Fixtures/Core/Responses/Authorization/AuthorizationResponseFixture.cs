#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Responses.Authorization;
using Lumina.Domain.SharedKernel.Common.Enums.Authorization;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Responses.Authorization;

/// <summary>
/// Fixture class for the <see cref="AuthorizationResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class AuthorizationResponseFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="AuthorizationResponse"/>.
    /// </summary>
    /// <param name="userId">Optional. The Id of the user.</param>
    /// <param name="role">Optional. The role of the user.</param>
    /// <param name="permissions">Optional. The permissions of the user.</param>
    /// <returns>The created <see cref="AuthorizationResponse"/>.</returns>
    public AuthorizationResponse Create(
        Guid? userId = null, 
        string? role = null, 
        IReadOnlySet<AuthorizationPermission>? permissions = null)
    {
        return new AuthorizationResponse(
            userId ?? Guid.NewGuid(),
            role,
            permissions ?? new HashSet<AuthorizationPermission>());
    }

    /// <summary>
    /// Creates a list of <see cref="AuthorizationResponse"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<AuthorizationResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
