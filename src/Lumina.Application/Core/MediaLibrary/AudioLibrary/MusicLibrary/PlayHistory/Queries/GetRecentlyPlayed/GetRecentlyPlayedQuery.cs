#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DTO.Pagination;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory.Queries.GetRecentlyPlayed;

/// <summary>
/// Query for getting the recently played tracks of the current user, from the most recently played.
/// </summary>
/// <param name="PaginationData">The object containing the requested pagination data, or <see langword="null"/> to return every matching track.</param>
[DebuggerDisplay("PaginationData: {PaginationData}")]
public record GetRecentlyPlayedQuery(
    PaginationDataDto? PaginationData
) : IQuery;
