#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Responses.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Responses.Plugins;

/// <summary>
/// Fixture class for the <see cref="LibraryBookReaderResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryBookReaderResponseFixture
{
    /// <summary>
    /// Creates a random valid <see cref="LibraryBookReaderResponse"/>.
    /// </summary>
    /// <param name="pluginId">Optional. The unique identifier of the plugin providing the book reader.</param>
    /// <param name="name">Optional. The display name of the book reader.</param>
    /// <param name="supportedExtensions">Optional. The file extensions supported by the book reader.</param>
    /// <param name="isEnabled">Optional. Whether the book reader is enabled for the media library.</param>
    /// <returns>The created <see cref="LibraryBookReaderResponse"/>.</returns>
    public LibraryBookReaderResponse Create(
        Guid? pluginId = null,
        string? name = null,
        IReadOnlyList<string>? supportedExtensions = null,
        bool? isEnabled = null)
    {
        return new LibraryBookReaderResponse(
            pluginId ?? Guid.NewGuid(),
            name ?? $"Reader {Guid.NewGuid():N}",
            supportedExtensions ?? [".epub", ".pdf"],
            isEnabled ?? true
        );
    }

    /// <summary>
    /// Creates a list of <see cref="LibraryBookReaderResponse"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<LibraryBookReaderResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
