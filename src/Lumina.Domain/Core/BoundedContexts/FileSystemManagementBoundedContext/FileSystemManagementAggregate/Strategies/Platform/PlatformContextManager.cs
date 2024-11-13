#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Enums.FileSystem;
using System;
using System.Runtime.InteropServices;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Strategies.Platform;

/// <summary>
/// Service for managing the platform context.
/// </summary>
public class PlatformContextManager : IPlatformContextManager
{
    private IPlatformContext? _currentPlatformContext;
    private readonly IPlatformContextFactory _platformContextFactory;
    private readonly IOperatingSystemInfo _operatingSystemInfo;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlatformContextManager"/> class.
    /// </summary>
    /// <param name="platformContextFactory">Injected factory for creating platform contexts.</param>
    /// <param name="operatingSystemInfo">Injected class for checking the platform type of the current Operating System.</param>
    public PlatformContextManager(IPlatformContextFactory platformContextFactory, IOperatingSystemInfo operatingSystemInfo)
    {
        _platformContextFactory = platformContextFactory;
        _operatingSystemInfo = operatingSystemInfo;
    }

    /// <summary>
    /// Gets the current platform context.
    /// </summary>
    /// <returns>The current platform context.</returns>
    public IPlatformContext GetCurrentContext()
    {
        // set a default context if none is set
        if (_currentPlatformContext is null)
            if (_operatingSystemInfo.IsOSPlatform(OSPlatform.Linux) || _operatingSystemInfo.IsOSPlatform(OSPlatform.OSX))
                SetCurrentPlatform(PlatformType.Unix);
            else
                SetCurrentPlatform(PlatformType.Windows);
        return _currentPlatformContext!;
    }

    /// <summary>
    /// Sets the current platform context.
    /// </summary>
    /// <param name="platformType">The platform to be set.</param>
    /// <exception cref="ArgumentException">Thrown when an unsupported platform type is provided.</exception>
    public void SetCurrentPlatform(PlatformType platformType)
    {
        // determine the correct context based on platformType
        _currentPlatformContext = platformType switch
        {
            PlatformType.Unix => _platformContextFactory.CreateStrategy<IUnixPlatformContext>(),
            PlatformType.Windows => _platformContextFactory.CreateStrategy<IWindowsPlatformContext>(),
            _ => throw new ArgumentException($"Unsupported platform type: {platformType}"),
        };
    }
}
