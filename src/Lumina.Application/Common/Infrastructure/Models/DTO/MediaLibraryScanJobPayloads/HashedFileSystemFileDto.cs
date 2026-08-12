#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
#endregion

namespace Lumina.Application.Common.Infrastructure.Models.DTO.MediaLibraryScanJobPayloads;

/// <summary>
/// Represents an item for the payload of the file hashing media library scan job.
/// </summary>
[DebuggerDisplay("Path = {Path}, CurrentHash = {CurrentHash}, OldHash = {OldHash}, Ticks = {Ticks}, Size = {Size}")]
[StructLayout(LayoutKind.Sequential)]
public struct HashedFileSystemFileDto : IEquatable<HashedFileSystemFileDto>
{
    /// <summary>
    /// The path of the file system file.
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    /// The size of the file system file.
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// The current hash obtained by sampling the file system file contents.
    /// </summary>
    public ulong CurrentHash { get; set; }

    /// <summary>
    /// The old hash of the file system file, stored at the previous scan.
    /// </summary>
    public ulong OldHash { get; set; }

    /// <summary>
    /// The time and date when the file system file was last modified, stored in ticks.
    /// </summary>
    public long Ticks { get; set; }

    /// <inheritdoc/>
    public readonly bool Equals(HashedFileSystemFileDto other)
    {
        return Size == other.Size && CurrentHash == other.CurrentHash && OldHash == other.OldHash && Ticks == other.Ticks && Path == other.Path;
    }

    /// <inheritdoc/>
    public override readonly bool Equals(object? obj)
    {
        return obj is HashedFileSystemFileDto item && Equals(item);
    }

    /// <inheritdoc/>
    public static bool operator ==(HashedFileSystemFileDto left, HashedFileSystemFileDto right)
    {
        return left.Equals(right);
    }

    /// <inheritdoc/>
    public static bool operator !=(HashedFileSystemFileDto left, HashedFileSystemFileDto right)
    {
        return !(left == right);
    }

    /// <inheritdoc/>
    public override readonly int GetHashCode()
    {
        return HashCode.Combine(Size, CurrentHash, OldHash, Ticks, Path);
    }
}
