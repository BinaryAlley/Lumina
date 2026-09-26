#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.FileSystemManagement.Paths.Queries.CheckPathExists;

/// <summary>
/// Query for checking the existence of a file system path.
/// </summary>
/// <param name="Path">The path for which to check the existence of.</param>
/// <param name="ShouldIncludeHiddenElements">Whether to include hidden file system elements or not.</param>
[DebuggerDisplay("Path: {Path}, ShouldIncludeHiddenElements: {ShouldIncludeHiddenElements}")]
public record CheckPathExistsQuery(
    string? Path,
    bool ShouldIncludeHiddenElements
) : IQuery;
