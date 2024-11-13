#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Enums.FileSystem;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Strategies.Path;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Strategies.Platform;

/// <summary>
/// Interface for defining the contract of a platform-specific context.
/// </summary>
public interface IPlatformContext
{
    /// <summary>
    /// Gets the platform for the current platform context.
    /// </summary>
    PlatformType Platform { get; }

    /// <summary>
    /// Gets the path strategy for the current platform context.
    /// </summary>
    IPathStrategy PathStrategy { get; }
}
