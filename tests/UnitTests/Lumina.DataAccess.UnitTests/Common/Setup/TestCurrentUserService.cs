#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Authentication;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.DataAccess.UnitTests.Common.Setup;

/// <summary>
/// Fake service for retrieving the current user's information during tests.
/// </summary>
[ExcludeFromCodeCoverage]
public class TestCurrentUserService : ICurrentUserService
{
    private Guid userId;
    public Guid? UserId => userId;

    /// <summary>
    /// Sets the test value of the user id.
    /// </summary>
    /// <param name="userId">The user id to use during tests.</param>
    public void SetUserId(Guid userId)
    {
        this.userId = userId;
    }
}
