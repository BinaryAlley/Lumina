#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DTO.Pagination;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory.Queries.GetMostPlayed;

/// <summary>
/// Query for getting the most played tracks of the current user, from the most played.
/// </summary>
/// <param name="PaginationData">The object containing the requested pagination data, or <see langword="null"/> to return every matching track.</param>
[DebuggerDisplay("PaginationData: {PaginationData}")]
public record GetMostPlayedQuery(
    PaginationDataDto? PaginationData
) : IQuery;
