#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Requests.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.Plugins;

/// <summary>
/// Fixture class for the <see cref="SetBookReaderEnabledRequest"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SetBookReaderEnabledRequestFixture
{
    /// <summary>
    /// Creates a random valid <see cref="SetBookReaderEnabledRequest"/>.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the media library whose book reader is enabled or disabled.</param>
    /// <param name="pluginId">Optional. The unique identifier of the plugin providing the book reader.</param>
    /// <param name="isEnabled">Optional. Whether the book reader should be enabled for the media library.</param>
    /// <returns>The created <see cref="SetBookReaderEnabledRequest"/>.</returns>
    public SetBookReaderEnabledRequest Create(
        Guid? libraryId = null,
        Guid? pluginId = null,
        bool? isEnabled = null)
    {
        return new SetBookReaderEnabledRequest
        {
            LibraryId = libraryId ?? Guid.NewGuid(),
            PluginId = pluginId ?? Guid.NewGuid(),
            IsEnabled = isEnabled ?? true
        };
    }

    /// <summary>
    /// Creates a list of <see cref="SetBookReaderEnabledRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<SetBookReaderEnabledRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
