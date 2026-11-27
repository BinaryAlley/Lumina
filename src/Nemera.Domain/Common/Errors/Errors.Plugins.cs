#region ========================================================================= USING =====================================================================================
using Nemera.Domain.Common.Primitives;
#endregion

namespace Nemera.Domain.Common.Errors;

/// <summary>
/// Plugin error types.
/// </summary>
public static partial class Errors
{
    /// <summary>
    /// Groups the errors that report plugin rule violations and plugin loading failures.
    /// </summary>
    public static class Plugins
    {
        /// <summary>
        /// Gets the error that reports that the requested plugin was not found.
        /// </summary>
        public static Error PluginNotFound => Error.NotFound(description: nameof(PluginNotFound));

        /// <summary>
        /// Gets the error that reports that the plugin identifier cannot be empty.
        /// </summary>
        public static Error PluginIdCannotBeEmpty => Error.Validation(description: nameof(PluginIdCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that the plugin settings must not be <see langword="null"/>.
        /// </summary>
        public static Error PluginSettingsCannotBeNull => Error.Validation(description: nameof(PluginSettingsCannotBeNull));

        /// <summary>
        /// Gets the error that reports that the library identifier cannot be empty.
        /// </summary>
        public static Error LibraryIdCannotBeEmpty => Error.Validation(description: nameof(LibraryIdCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that the plugin identifiers must be unique.
        /// </summary>
        public static Error PluginIdsMustBeUnique => Error.Validation(description: nameof(PluginIdsMustBeUnique));

        /// <summary>
        /// Gets the error that reports that the plugin is not supported for the library type.
        /// </summary>
        public static Error PluginNotSupportedForLibraryType => Error.Validation(description: nameof(PluginNotSupportedForLibraryType));

        /// <summary>
        /// Gets the error that reports that the metadata provider configuration of the library was not found.
        /// </summary>
        public static Error LibraryMetadataProviderConfigurationNotFound => Error.NotFound(description: nameof(LibraryMetadataProviderConfigurationNotFound));

        /// <summary>
        /// Gets the error that reports that the plugin archive must not be <see langword="null"/>.
        /// </summary>
        public static Error PluginArchiveCannotBeNull => Error.Validation(description: nameof(PluginArchiveCannotBeNull));

        /// <summary>
        /// Gets the error that reports that the plugin file name cannot be empty.
        /// </summary>
        public static Error PluginFileNameCannotBeEmpty => Error.Validation(description: nameof(PluginFileNameCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that the plugin file type is not supported.
        /// </summary>
        public static Error UnsupportedPluginFileType => Error.Validation(description: nameof(UnsupportedPluginFileType));

        /// <summary>
        /// Gets the error that reports that the plugin archive could not be read.
        /// </summary>
        public static Error PluginArchiveNotReadable => Error.Failure(description: nameof(PluginArchiveNotReadable));

        /// <summary>
        /// Gets the error that reports that the plugin archive contains no assemblies.
        /// </summary>
        public static Error PluginArchiveContainsNoAssemblies => Error.Failure(description: nameof(PluginArchiveContainsNoAssemblies));
    }
}
