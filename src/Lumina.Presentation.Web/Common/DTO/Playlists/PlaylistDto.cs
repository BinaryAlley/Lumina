#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.Enums.Playlists;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Playlists;

/// <summary>
/// Data transfer object for a playlist together with a page of its ordered entries.
/// </summary>
/// <param name="Id">The Id of the playlist.</param>
/// <param name="Name">The name of the playlist.</param>
/// <param name="Description">The description of the playlist, if applicable.</param>
/// <param name="Type">The kind of media the playlist holds.</param>
/// <param name="Entries">The page of ordered entries of the playlist, together with its pagination metadata.</param>
/// <param name="CreatedOnUtc">The date and time when the playlist was created.</param>
/// <param name="UpdatedOnUtc">The date and time when the playlist was last updated, if applicable.</param>
[DebuggerDisplay("Id: {Id}; Name: {Name}; Entries: {Entries.Count}")]
public sealed record PlaylistDto(
    Guid Id,
    string Name,
    string? Description,
    PlaylistType Type,
    PaginatedDto<PlaylistEntryDto> Entries,
    DateTime CreatedOnUtc,
    DateTime? UpdatedOnUtc
);
