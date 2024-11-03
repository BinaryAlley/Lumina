#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Enums.FileSystem;
using System;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Strategies.Platform;

/// <summary>
/// Interface for managing the platform context.
/// </summary>
public interface IPlatformContextManager
{
    /// <summary>
    /// Gets the current platform context.
    /// </summary>
    /// <returns>The current platform context.</returns>
    IPlatformContext GetCurrentContext();

    /// <summary>
    /// Sets the current platform context.
    /// </summary>
    /// <param name="platformType">The platform to be set.</param>
    /// <exception cref="ArgumentException">Thrown when an unsupported platform type is provided.</exception>
    void SetCurrentPlatform(PlatformType platformType);
}
