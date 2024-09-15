#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Enums.FileSystem;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Strategies.Path;
#endregion

namespace Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Strategies.Platform;

/// <summary>
/// Interface for defining the contract of a platform-specific context.
/// </summary>
public interface IPlatformContext
{
    #region ==================================================================== PROPERTIES =================================================================================
    /// <summary>
    /// Gets the platform for the current platform context.
    /// </summary>
    PlatformType Platform { get; }

    /// <summary>
    /// Gets the path strategy for the current platform context.
    /// </summary>
    IPathStrategy PathStrategy { get; }
    #endregion
}