#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.Enums.Authorization;
using Lumina.Presentation.Web.Common.Responses.Authorization;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Responses.Authorization;

/// <summary>
/// Fixture class for generating <see cref="GetAuthorizationResponse"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetAuthorizationResponseFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="GetAuthorizationResponse"/> instance with randomized test data.
    /// </summary>
    /// <param name="userId">Optional unique identifier of the user.</param>
    /// <param name="role">Optional role associated to the user.</param>
    /// <param name="permissions">Optional collection of permissions associated to the user.</param>
    /// <returns>A configured <see cref="GetAuthorizationResponse"/> instance.</returns>
    public GetAuthorizationResponse Create(
        Guid? userId = null,
        string? role = null,
        AuthorizationPermission[]? permissions = null)
    {
        return new GetAuthorizationResponse(
            UserId: userId ?? Guid.NewGuid(),
            Role: role ?? _faker.Name.JobTitle(),
            Permissions: permissions ?? [AuthorizationPermission.CanViewUsers]
        );
    }

    /// <summary>
    /// Creates multiple <see cref="GetAuthorizationResponse"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="GetAuthorizationResponse"/> instances.</returns>
    public List<GetAuthorizationResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
