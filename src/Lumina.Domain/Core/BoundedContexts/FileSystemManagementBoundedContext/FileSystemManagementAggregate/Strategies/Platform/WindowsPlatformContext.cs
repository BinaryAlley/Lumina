#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Enums.FileSystem;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Strategies.Path;

#endregion

namespace Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Strategies.Platform;

/// <summary>
/// Platform context for Windows platforms.
/// </summary>
public class WindowsPlatformContext : IWindowsPlatformContext
{
    /// <summary>
    /// Gets the platform type, which is Windows for this context.
    /// </summary>
    public PlatformType Platform { get; } = PlatformType.Windows;

    /// <summary>
    /// Gets the path strategy for Windows platforms.
    /// </summary>
    public IPathStrategy PathStrategy { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="WindowsPlatformContext"/> class.
    /// </summary>
    /// <param name="windowsPathStrategy">Injected service for creating path strategies for Windows platforms.</param>
    public WindowsPlatformContext(IWindowsPathStrategy windowsPathStrategy)
    {
        PathStrategy = windowsPathStrategy;
    }
}
