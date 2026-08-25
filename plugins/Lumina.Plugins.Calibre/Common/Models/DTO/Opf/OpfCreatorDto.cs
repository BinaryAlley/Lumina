#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Plugins.Calibre.Common.Models.DTO.Opf;

/// <summary>
/// Data transfer object for a creator of a book read from an OPF file.
/// </summary>
/// <param name="Name">The name of the creator.</param>
/// <param name="Role">The role code of the creator, for example <c>aut</c> for author.</param>
[DebuggerDisplay("Name: {Name}")]
internal sealed record OpfCreatorDto(
    string Name,
    string? Role
);
