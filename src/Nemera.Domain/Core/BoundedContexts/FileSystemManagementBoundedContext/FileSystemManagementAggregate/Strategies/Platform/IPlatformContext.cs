#region ========================================================================= USING =====================================================================================
using Nemera.Domain.SharedKernel.Common.Enums.FileSystem;
using Nemera.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Strategies.Path;
#endregion

namespace Nemera.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Strategies.Platform;

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
