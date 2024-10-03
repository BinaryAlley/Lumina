﻿#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.FileManagement;

/// <summary>
/// Represents a response to the inquiry about the existence of a file system path.
/// </summary>
/// <param name="Exists">Whether a file system path exists or not.</param>
[DebuggerDisplay("{Exists}")]
public record PathExistsResponse(
    bool Exists
);