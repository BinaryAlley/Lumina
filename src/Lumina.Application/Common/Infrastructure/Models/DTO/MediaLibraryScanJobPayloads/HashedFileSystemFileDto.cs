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

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns><see langword="true"/> if the current object is equal to the <paramref name="other"/> parameter, <see langword="false"/> otherwise.</returns>
    public readonly bool Equals(HashedFileSystemFileDto other)
    {
        return Size == other.Size && CurrentHash == other.CurrentHash && OldHash == other.OldHash && Ticks == other.Ticks && Path == other.Path;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns><see langword="true"/> if the specified object is equal to the current object, <see langword="false"/> otherwise.</returns>
    public override readonly bool Equals(object? obj)
    {
        return obj is HashedFileSystemFileDto item && Equals(item);
    }

    /// <summary>
    /// Custom implementation of the equality operator.
    /// </summary>
    /// <param name="left">The left operand of equality.</param>
    /// <param name="right">The right operand of equality.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is equal to <paramref name="right"/>, <see langword="false"/> otherwise.</returns>
    public static bool operator ==(HashedFileSystemFileDto left, HashedFileSystemFileDto right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Custom implementation of the inequality operator.
    /// </summary>
    /// <param name="left">The left operand of equality.</param>
    /// <param name="right">The right operand of equality.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is not equal to <paramref name="right"/>, <see langword="false"/> otherwise.</returns>
    public static bool operator !=(HashedFileSystemFileDto left, HashedFileSystemFileDto right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    public override readonly int GetHashCode()
    {
        return HashCode.Combine(Size, CurrentHash, OldHash, Ticks, Path);
    }
}
