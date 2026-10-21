#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.Playlists;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.Playlists;

/// <summary>
/// Represents a response for a playlist together with its ordered entries.
/// </summary>
/// <param name="Id">The Id of the playlist.</param>
/// <param name="Name">The name of the playlist.</param>
/// <param name="Description">The description of the playlist, if applicable.</param>
/// <param name="Type">The kind of media the playlist holds.</param>
/// <param name="Entries">The ordered entries of the requested window of the playlist.</param>
/// <param name="EntryCount">The number of entries the playlist holds, even when only a window of them is returned.</param>
/// <param name="CreatedOnUtc">The date and time when the playlist was created.</param>
/// <param name="UpdatedOnUtc">The date and time when the playlist was last updated, if applicable.</param>
[DebuggerDisplay("Id: {Id}; Name: {Name}; EntryCount: {EntryCount}; Window: {Entries.Count}")]
public sealed record PlaylistResponse(
    Guid Id,
    string Name,
    string? Description,
    PlaylistType Type,
    IReadOnlyList<PlaylistEntryResponse> Entries,
    int EntryCount,
    DateTime CreatedOnUtc,
    DateTime? UpdatedOnUtc
);
