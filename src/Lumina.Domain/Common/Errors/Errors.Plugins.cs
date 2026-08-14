#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
#endregion

namespace Lumina.Domain.Common.Errors;

/// <summary>
/// Plugin error types.
/// </summary>
public static partial class Errors
{
    public static class Plugins
    {
        public static Error PluginNotFound => Error.NotFound(description: nameof(PluginNotFound));
        public static Error PluginIdCannotBeEmpty => Error.Validation(description: nameof(PluginIdCannotBeEmpty));
        public static Error PluginSettingsCannotBeNull => Error.Validation(description: nameof(PluginSettingsCannotBeNull));
        public static Error LibraryIdCannotBeEmpty => Error.Validation(description: nameof(LibraryIdCannotBeEmpty));
        public static Error PluginIdsListCannotBeNull => Error.Validation(description: nameof(PluginIdsListCannotBeNull));
        public static Error PluginIdsListCannotBeEmpty => Error.Validation(description: nameof(PluginIdsListCannotBeEmpty));
        public static Error LibraryMetadataProviderConfigurationNotFound => Error.NotFound(description: nameof(LibraryMetadataProviderConfigurationNotFound));
    }
}
