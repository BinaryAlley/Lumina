#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.Plugins.Commands.SetLibraryMetadataProviderEnabled;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.Plugins.Commands.SetLibraryMetadataProviderEnabled;

/// <summary>
/// Fixture class for the <see cref="SetLibraryMetadataProviderEnabledCommand"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SetLibraryMetadataProviderEnabledCommandFixture
{
    /// <summary>
    /// Creates a random valid command to enable or disable a metadata provider for a media library.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the media library whose metadata provider is enabled or disabled.</param>
    /// <param name="pluginId">Optional. The Id of the plugin providing the metadata.</param>
    /// <param name="isEnabled">Whether the metadata provider should be enabled for the media library.</param>
    /// <returns>The created command.</returns>
    public SetLibraryMetadataProviderEnabledCommand Create(Guid? libraryId = null, Guid? pluginId = null, bool isEnabled = true)
    {
        return new Faker<SetLibraryMetadataProviderEnabledCommand>()
            .CustomInstantiator(f => new SetLibraryMetadataProviderEnabledCommand(
                default,
                default,
                true))
            .RuleFor(x => x.LibraryId, libraryId ?? Guid.NewGuid())
            .RuleFor(x => x.PluginId, pluginId ?? Guid.NewGuid())
            .RuleFor(x => x.IsEnabled, isEnabled)
            .Generate();
    }

    /// <summary>
    /// Creates a list of <see cref="SetLibraryMetadataProviderEnabledCommand"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<SetLibraryMetadataProviderEnabledCommand> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
