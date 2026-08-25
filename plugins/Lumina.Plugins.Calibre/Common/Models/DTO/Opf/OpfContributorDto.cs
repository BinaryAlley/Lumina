#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Plugins.Calibre.Common.Models.DTO.Opf;

/// <summary>
/// Data transfer object for a contributor of a book read from an OPF file.
/// </summary>
/// <param name="Name">The name of the contributor.</param>
/// <param name="Role">The role code of the contributor, for example <c>bkp</c> for the book producer.</param>
[DebuggerDisplay("Name: {Name}")]
internal sealed record OpfContributorDto(
    string Name,
    string? Role
);
