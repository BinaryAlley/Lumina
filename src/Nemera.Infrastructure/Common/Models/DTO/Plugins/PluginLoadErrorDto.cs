#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Infrastructure.Common.Models.DTO.Plugins;

/// <summary>
/// Data transfer object for the error that occurred while loading a plugin assembly.
/// </summary>
/// <param name="AssemblyName">The file name of the plugin assembly that failed to load, without its extension.</param>
/// <param name="ErrorMessage">The error message describing the load failure.</param>
[DebuggerDisplay("AssemblyName: {AssemblyName}")]
internal sealed record PluginLoadErrorDto(
    string AssemblyName, 
    string ErrorMessage
);
