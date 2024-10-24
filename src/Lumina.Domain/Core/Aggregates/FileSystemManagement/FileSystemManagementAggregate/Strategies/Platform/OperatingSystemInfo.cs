#region ========================================================================= USING =====================================================================================
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
#endregion

namespace Lumina.Domain.Core.Aggregates.FileSystemManagement.FileSystemManagementAggregate.Strategies.Platform;

/// <summary>
/// Class for checking the type of the current Operating System platform.
/// </summary>
[ExcludeFromCodeCoverage]
internal class OperatingSystemInfo : IOperatingSystemInfo
{
    /// <summary>
    /// Determines if the current Operating System plaform is <paramref name="platform"/>.
    /// </summary>
    /// <param name="platform">The Operating System platform to check against.</param>
    /// <returns><see langword="true"/> if tthe current Operating System platform is equal to <paramref name="platform"/>, <see langword="false"/> otherwise.</returns>
    public bool IsOSPlatform(OSPlatform platform)
    {
        return RuntimeInformation.IsOSPlatform(platform);
    }
}
