#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Enums.FileSystem;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Strategies.Path;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Strategies.Platform;

/// <summary>
/// Platform context for UNIX platforms.
/// </summary>
internal class UnixPlatformContext : IUnixPlatformContext
{
    /// <summary>
    /// Gets the platform type, which is Unix for this context.
    /// </summary>
    public PlatformType Platform { get; } = PlatformType.Unix;

    /// <summary>
    /// Gets the path strategy for Unix platforms.
    /// </summary>
    public IPathStrategy PathStrategy { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnixPlatformContext"/> class.
    /// </summary>
    /// <param name="unixPathStrategy">Injected service for creating path strategies for UNIX platforms.</param>
    public UnixPlatformContext(IUnixPathStrategy unixPathStrategy)
    {
        PathStrategy = unixPathStrategy;
    }
}
