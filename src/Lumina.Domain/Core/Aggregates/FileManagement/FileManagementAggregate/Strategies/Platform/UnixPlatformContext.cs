#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Enums.FileSystem;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Strategies.Path;
#endregion

namespace Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Strategies.Platform;

/// <summary>
/// Platform context for UNIX platforms.
/// </summary>
internal class UnixPlatformContext : IUnixPlatformContext
{
    #region ==================================================================== PROPERTIES =================================================================================
    /// <summary>
    /// Gets the platform type, which is Unix for this context.
    /// </summary>
    public PlatformType Platform { get; } = PlatformType.Unix;

    /// <summary>
    /// Gets the path strategy for Unix platforms.
    /// </summary>
    public IPathStrategy PathStrategy { get; private set; }
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="UnixPlatformContext"/> class.
    /// </summary>
    /// <param name="unixPathStrategy">Injected service for creating path strategies for UNIX platforms.</param>
    public UnixPlatformContext(IUnixPathStrategy unixPathStrategy)
    {
        PathStrategy = unixPathStrategy;
    }
    #endregion
}