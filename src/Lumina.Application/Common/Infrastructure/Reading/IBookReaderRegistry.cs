#region ========================================================================= USING =====================================================================================
using System;
using System.Collections.Generic;
#endregion

namespace Lumina.Application.Common.Infrastructure.Reading;

/// <summary>
/// Registry of the book readers provided by the loaded plugins.
/// </summary>
public interface IBookReaderRegistry
{
    /// <summary>
    /// Gets the file extensions supported by the book readers of each loaded plugin, keyed by the Id of the plugin.
    /// </summary>
    /// <returns>The supported extensions of the book readers, keyed by the Id of the plugin providing them.</returns>
    IReadOnlyDictionary<Guid, IReadOnlyList<string>> GetSupportedExtensionsByPluginId();
}
