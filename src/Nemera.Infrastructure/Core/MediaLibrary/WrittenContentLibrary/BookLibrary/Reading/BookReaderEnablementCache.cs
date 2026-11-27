#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Reading;
using System;
using System.Collections.Concurrent;
#endregion

namespace Lumina.Infrastructure.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;

/// <summary>
/// In-memory cache of whether the book reader of a plugin is enabled for a media library. The entries are invalidated when the
/// enablement is changed by the user, or when the configurations of a plugin or a library are removed.
/// </summary>
internal sealed class BookReaderEnablementCache : IBookReaderEnablementCache
{
    private readonly ConcurrentDictionary<(Guid LibraryId, Guid PluginId), bool> _enablements = [];

    /// <inheritdoc/>
    public bool? Get(Guid libraryId, Guid pluginId)
    {
        return _enablements.TryGetValue((libraryId, pluginId), out bool isEnabled) ? isEnabled : null;
    }

    /// <inheritdoc/>
    public void Set(Guid libraryId, Guid pluginId, bool isEnabled)
    {
        _enablements[(libraryId, pluginId)] = isEnabled;
    }

    /// <inheritdoc/>
    public void Invalidate(Guid libraryId, Guid pluginId)
    {
        _enablements.TryRemove((libraryId, pluginId), out _);
    }

    /// <inheritdoc/>
    public void InvalidateLibrary(Guid libraryId)
    {
        foreach ((Guid cachedLibraryId, Guid cachedPluginId) in _enablements.Keys)
            if (cachedLibraryId == libraryId)
                _enablements.TryRemove((cachedLibraryId, cachedPluginId), out _);
    }

    /// <inheritdoc/>
    public void InvalidatePlugin(Guid pluginId)
    {
        foreach ((Guid cachedLibraryId, Guid cachedPluginId) in _enablements.Keys)
            if (cachedPluginId == pluginId)
                _enablements.TryRemove((cachedLibraryId, cachedPluginId), out _);
    }
}
