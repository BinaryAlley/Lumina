#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
#endregion

namespace Lumina.Presentation.Api.Fixtures.Core.Endpoints.Plugins;

/// <summary>
/// Fixture class for building plugin archives used by the plugin integration tests.
/// </summary>
[ExcludeFromCodeCoverage]
public class TestPluginArchiveFixture
{
    /// <summary>
    /// Builds a ZIP archive in memory containing a single plugin assembly entry.
    /// </summary>
    /// <param name="dllName">Optional name of the plugin assembly entry; a unique name is generated when not supplied.</param>
    /// <returns>The bytes of the plugin ZIP archive.</returns>
    public byte[] CreateZip(string? dllName = null)
    {
        string resolvedDllName = dllName ?? $"test-plugin-{Guid.NewGuid():N}.dll";

        using MemoryStream stream = new();
        using (ZipArchive archive = new(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            ZipArchiveEntry dllEntry = archive.CreateEntry(resolvedDllName);
            using Stream entryStream = dllEntry.Open();
            entryStream.Write(CreateAssemblyBytes());
        }

        return stream.ToArray();
    }

    /// <summary>
    /// Builds a single plugin assembly file in memory.
    /// </summary>
    /// <returns>The bytes of the plugin assembly.</returns>
    public byte[] CreateDll()
    {
        return CreateAssemblyBytes();
    }

    /// <summary>
    /// Builds the arbitrary bytes of a plugin assembly file.
    /// </summary>
    /// <returns>The plugin assembly bytes.</returns>
    private static byte[] CreateAssemblyBytes()
    {
        return [0x4D, 0x5A, 0x90, 0x00, 0x03, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00];
    }
}
