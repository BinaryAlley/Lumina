#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Domain.SharedKernel.Common.Enums.Favorites;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.Favorites.Queries.GetFavorites;

/// <summary>
/// Query for getting the favorites of the current user, from the most recently favorited.
/// </summary>
/// <param name="MediaKind">The kind of media item to restrict the favorites to, or <see langword="null"/> for every kind.</param>
/// <param name="PaginationData">The object containing the requested pagination data.</param>
[DebuggerDisplay("MediaKind: {MediaKind}")]
public record GetFavoritesQuery(
    FavoriteMediaKind? MediaKind,
    PaginationDataDto? PaginationData
) : IQuery;
