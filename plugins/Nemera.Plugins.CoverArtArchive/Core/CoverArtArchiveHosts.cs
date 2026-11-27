#region ========================================================================= USING =====================================================================================
using System;
#endregion

namespace Lumina.Plugins.CoverArtArchive.Core;

/// <summary>
/// Defines the hosts of the Cover Art Archive and the Internet Archive, which are the only hosts the plugin queries, the only hosts it may follow a
/// redirect to, and the only hosts it may fetch artwork from. The Cover Art Archive is not self hostable, so no other host can be a legitimate source.
/// </summary>
internal static class CoverArtArchiveHosts
{
    /// <summary>
    /// Determines whether the provided host is one of the hosts the Cover Art Archive may delegate its responses to.
    /// </summary>
    /// <param name="host">The host to check.</param>
    /// <returns><see langword="true"/> when the host is allowed, otherwise <see langword="false"/>.</returns>
    public static bool IsAllowed(string host)
    {
        return host.Equals("coverartarchive.org", StringComparison.OrdinalIgnoreCase)
            || host.EndsWith(".coverartarchive.org", StringComparison.OrdinalIgnoreCase)
            || host.Equals("archive.org", StringComparison.OrdinalIgnoreCase)
            || host.EndsWith(".archive.org", StringComparison.OrdinalIgnoreCase);
    }
}
