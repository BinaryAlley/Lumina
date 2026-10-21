#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.Playlists;
using System;
#endregion

namespace Lumina.Application.Common.DTO.Playlists;

/// <summary>
/// Data transfer object for a playlist in a list, without its entries.
/// </summary>
/// <param name="Id">The Id of the playlist.</param>
/// <param name="Name">The name of the playlist.</param>
/// <param name="Description">The description of the playlist, if applicable.</param>
/// <param name="Type">The kind of media the playlist holds.</param>
/// <param name="EntryCount">The number of entries of the playlist.</param>
/// <param name="CreatedOnUtc">The date and time when the playlist was created.</param>
/// <param name="UpdatedOnUtc">The date and time when the playlist was last updated, if applicable.</param>
public sealed record PlaylistSummaryDto(
    Guid Id,
    string Name,
    string? Description,
    PlaylistType Type,
    int EntryCount,
    DateTime CreatedOnUtc,
    DateTime? UpdatedOnUtc
);
