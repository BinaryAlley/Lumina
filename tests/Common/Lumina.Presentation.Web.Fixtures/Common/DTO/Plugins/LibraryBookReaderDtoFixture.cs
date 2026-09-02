#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.Plugins;

/// <summary>
/// Fixture class for generating <see cref="LibraryBookReaderDto"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryBookReaderDtoFixture
{
    /// <summary>
    /// Creates a new <see cref="LibraryBookReaderDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="pluginId">Optional unique identifier of the plugin providing the book reader.</param>
    /// <param name="name">Optional display name of the book reader.</param>
    /// <param name="supportedExtensions">Optional file extensions supported by the book reader.</param>
    /// <param name="isEnabled">Optional value indicating whether the book reader is enabled for the media library.</param>
    /// <returns>A configured <see cref="LibraryBookReaderDto"/> instance.</returns>
    public LibraryBookReaderDto Create(
        Guid? pluginId = null,
        string? name = null,
        List<string>? supportedExtensions = null,
        bool? isEnabled = null)
    {
        return new LibraryBookReaderDto
        {
            PluginId = pluginId ?? Guid.NewGuid(),
            Name = name ?? $"Reader {Guid.NewGuid():N}",
            SupportedExtensions = supportedExtensions ?? [".epub", ".pdf"],
            IsEnabled = isEnabled ?? true
        };
    }

    /// <summary>
    /// Creates multiple <see cref="LibraryBookReaderDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="LibraryBookReaderDto"/> instances.</returns>
    public List<LibraryBookReaderDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
