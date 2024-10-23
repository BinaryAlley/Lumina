#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Enums.PhotoLibrary;
using Lumina.Domain.Common.Models.Core;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.Aggregates.FileSystemManagement.FileSystemManagementAggregate.ValueObjects;

/// <summary>
/// Value object for file system thumbnails.
/// </summary>
[DebuggerDisplay("{Type}")]
public class Thumbnail : ValueObject
{
    /// <summary>
    /// Gets the type of the image represented by this thumbnail.
    /// </summary>
    public ImageType Type { get; }

    /// <summary>
    /// Gets the bytes of the image represented by this thumbnail.
    /// </summary>
    public byte[] Bytes { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Thumbnail"/> class.
    /// </summary>
    /// <param name="type">The type of the image represented by this thumbnail.</param>
    /// <param name="bytes">The bytes of the image represented by this thumbnail.</param>
    public Thumbnail(ImageType type, byte[] bytes)
    {
        Type = type;
        Bytes = bytes;
    }

    /// <inheritdoc/>
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Type;
        yield return Bytes;
    }
}
