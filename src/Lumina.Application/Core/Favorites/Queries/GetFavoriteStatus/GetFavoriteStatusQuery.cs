#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Domain.SharedKernel.Common.Enums.Favorites;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.Favorites.Queries.GetFavoriteStatus;

/// <summary>
/// Query for finding out which media items of one kind are favorites of the current user.
/// </summary>
/// <param name="MediaKind">The kind of the media items to check.</param>
/// <param name="MediaItemIds">The unique identifiers of the media items to check.</param>
[DebuggerDisplay("MediaKind: {MediaKind}")]
public record GetFavoriteStatusQuery(
    FavoriteMediaKind? MediaKind,
    IReadOnlyList<Guid>? MediaItemIds
) : IQuery;
