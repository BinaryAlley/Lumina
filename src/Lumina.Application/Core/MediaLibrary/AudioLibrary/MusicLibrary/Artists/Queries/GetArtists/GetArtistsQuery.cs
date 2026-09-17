#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Queries.GetArtists;

/// <summary>
/// Query for getting all the artists of a media library.
/// </summary>
/// <param name="LibraryId">The Id of the media library whose artists are retrieved.</param>
/// <param name="CurrentPage">The page of results to retrieve.</param>
/// <param name="PerPage">The maximum number of artists to retrieve per page.</param>
/// <param name="SearchTerm">The search term used to filter the artists by name.</param>
[DebuggerDisplay("LibraryId: {LibraryId}")]
public record GetArtistsQuery(
    Guid LibraryId,
    int? CurrentPage,
    int? PerPage,
    string? SearchTerm
) : IQuery;
