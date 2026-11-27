#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.Favorites;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.Favorites;

/// <summary>
/// Represents a request for finding out which media items of one kind are favorites of the current user.
/// </summary>
/// <param name="MediaKind">The kind of the media items to check. Required.</param>
/// <param name="MediaItemIds">The Ids of the media items to check, from 1 to 100. Required.</param>
[DebuggerDisplay("MediaKind: {MediaKind}")]
public record GetFavoriteStatusRequest(
    FavoriteMediaKind? MediaKind,
    List<Guid>? MediaItemIds
);
