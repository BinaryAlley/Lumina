#region ========================================================================= USING =====================================================================================
using System.Runtime.InteropServices;
#endregion

namespace Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Strategies.Platform;

/// <summary>
/// Interface for checking the type of the current Operating System platform.
/// </summary>
public interface IOperatingSystemInfo
{
    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Determines if the current Operating System plaform is <paramref name="platform"/>.
    /// </summary>
    /// <param name="platform">The Operating System platform to check against.</param>
    /// <returns><see langword="true"/> if tthe current Operating System platform is equal to <paramref name="platform"/>, <see langword="false"/> otherwise.</returns>
    bool IsOSPlatform(OSPlatform platform);
    #endregion
}
