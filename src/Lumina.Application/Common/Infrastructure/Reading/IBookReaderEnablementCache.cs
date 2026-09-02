#region ========================================================================= USING =====================================================================================
using System;
#endregion

namespace Lumina.Application.Common.Infrastructure.Reading;

/// <summary>
/// Cache of whether the book reader of a plugin is enabled for a media library. The enablement is read from the database on its first
/// use and cached afterwards, because it is consulted on every section and resource request, and it only changes when the user toggles
/// a reader or a plugin is uninstalled; those operations invalidate the affected entries.
/// </summary>
public interface IBookReaderEnablementCache
{
    /// <summary>
    /// Gets the cached enablement of the book reader of the plugin identified by <paramref name="pluginId"/> for the media library identified by <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose book reader enablement is retrieved.</param>
    /// <param name="pluginId">The Id of the plugin providing the book reader.</param>
    /// <returns>The cached enablement, or <see langword="null"/> when it is not cached yet.</returns>
    bool? Get(Guid libraryId, Guid pluginId);

    /// <summary>
    /// Caches the enablement of the book reader of the plugin identified by <paramref name="pluginId"/> for the media library identified by <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose book reader enablement is cached.</param>
    /// <param name="pluginId">The Id of the plugin providing the book reader.</param>
    /// <param name="isEnabled">Whether the book reader is enabled for the media library.</param>
    void Set(Guid libraryId, Guid pluginId, bool isEnabled);

    /// <summary>
    /// Removes the cached enablement of the book reader of the plugin identified by <paramref name="pluginId"/> for the media library identified by <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose book reader enablement is invalidated.</param>
    /// <param name="pluginId">The Id of the plugin providing the book reader.</param>
    void Invalidate(Guid libraryId, Guid pluginId);

    /// <summary>
    /// Removes every cached enablement of the media library identified by <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose book reader enablements are invalidated.</param>
    void InvalidateLibrary(Guid libraryId);

    /// <summary>
    /// Removes every cached enablement of the plugin identified by <paramref name="pluginId"/>.
    /// </summary>
    /// <param name="pluginId">The Id of the plugin whose book reader enablements are invalidated.</param>
    void InvalidatePlugin(Guid pluginId);
}
