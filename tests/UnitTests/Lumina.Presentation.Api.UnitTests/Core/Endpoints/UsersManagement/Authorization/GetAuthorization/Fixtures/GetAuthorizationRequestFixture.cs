#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Requests.Authorization;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.UsersManagement.Authorization.GetAuthorization.Fixtures;

/// <summary>
/// Fixture class for the <see cref="GetAuthorizationRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetAuthorizationRequestFixture
{
    /// <summary>
    /// Creates a <see cref="GetAuthorizationRequest"/> with default or specified values.
    /// </summary>
    /// <param name="userId">Optional user Id to use.</param>
    /// <returns>The created <see cref="GetAuthorizationRequest"/>.</returns>
    public static GetAuthorizationRequest Create(Guid? userId = null)
    {
        return new GetAuthorizationRequest(
            UserId: userId ?? Guid.NewGuid()
        );
    }
}
