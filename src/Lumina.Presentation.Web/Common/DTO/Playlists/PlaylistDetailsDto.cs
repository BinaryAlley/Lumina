#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Enums.Playlists;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Playlists;

/// <summary>
/// Data transfer object for the editable scalars of a playlist after a details update.
/// </summary>
/// <param name="Id">The Id of the playlist.</param>
/// <param name="Name">The name of the playlist.</param>
/// <param name="Description">The description of the playlist, if applicable.</param>
/// <param name="Type">The kind of media the playlist holds.</param>
/// <param name="UpdatedOnUtc">The date and time when the playlist was last updated, if applicable.</param>
[DebuggerDisplay("Id: {Id}; Name: {Name}")]
public sealed record PlaylistDetailsDto(
    Guid Id,
    string Name,
    string? Description,
    PlaylistType Type,
    DateTime? UpdatedOnUtc
);
