﻿#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.FileManagement;

/// <summary>
/// Represents a response to the inquiry about the validity of a file system path.
/// </summary>
/// <param name="IsValid">Whether a file system path is valid or not.</param>
[DebuggerDisplay("{IsValid}")]
public record PathValidResponse(
    bool IsValid
);