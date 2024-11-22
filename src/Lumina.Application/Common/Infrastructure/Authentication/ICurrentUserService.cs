#region ========================================================================= USING =====================================================================================
using System;
#endregion

namespace Lumina.Application.Common.Infrastructure.Authentication;

/// <summary>
/// Defines a contract for accessing the current user's information.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the unique identifier of the currently authenticated user. Returns <see langword="null"/> if no user is authenticated.
    /// </summary>
    Guid? UserId { get; }
}
