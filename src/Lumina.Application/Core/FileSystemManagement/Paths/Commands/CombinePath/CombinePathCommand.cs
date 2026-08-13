#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.FileSystemManagement.Paths.Commands.CombinePath;

/// <summary>
/// Command for combining two file system path.
/// </summary>
/// <param name="OriginalPath">The path to combine to.</param>
/// <param name="NewPath">The path to combine with.</param>
[DebuggerDisplay("OriginalPath: {OriginalPath}, NewPath: {NewPath}")]
public record CombinePathCommand(
    string? OriginalPath, 
    string? NewPath
) : ICommand;
