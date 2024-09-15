#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Models.Common;

/// <summary>
/// Represents a request to get tag information.
/// </summary>
/// <param name="Name">Gets the name of the tag.</param>
[DebuggerDisplay("{Name}")]
public record TagModel(
    string? Name
);