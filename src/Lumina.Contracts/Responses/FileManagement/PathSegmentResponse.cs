﻿#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.FileManagement;

/// <summary>
/// Represents a response containing a file system path.
/// </summary>
/// <param name="Path">The returned path.</param>
[DebuggerDisplay("{Path}")]
public record PathSegmentResponse(
    string Path
);