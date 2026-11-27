#region ========================================================================= USING =====================================================================================
using Microsoft.AspNetCore.Authorization;
#endregion

namespace Lumina.Presentation.Web.Common.Authorization;

/// <summary>
/// Defines a requirement for initialization status in the authorization process.
/// </summary>
public class InitializationRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Gets the timeout duration, in minutes, for caching the initialization check.
    /// </summary>
    public int CacheTimeoutMinutes { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="InitializationRequirement"/> class.
    /// </summary>
    /// <param name="cacheTimeoutMinutes">The cache timeout in minutes for the initialization status.</param>
    public InitializationRequirement(int cacheTimeoutMinutes = 5)
    {
        CacheTimeoutMinutes = cacheTimeoutMinutes;
    }
}
