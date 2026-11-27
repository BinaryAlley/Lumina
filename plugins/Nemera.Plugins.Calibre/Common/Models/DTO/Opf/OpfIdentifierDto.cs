#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Plugins.Calibre.Common.Models.DTO.Opf;

/// <summary>
/// Data transfer object for an identifier of a book read from an OPF file, with its scheme.
/// </summary>
/// <param name="Scheme">The scheme of the identifier, for example <c>ISBN</c> or <c>GOODREADS</c>.</param>
/// <param name="Value">The value of the identifier.</param>
[DebuggerDisplay("{Scheme}: {Value}")]
internal sealed record OpfIdentifierDto(
    string Scheme,
    string Value
);
