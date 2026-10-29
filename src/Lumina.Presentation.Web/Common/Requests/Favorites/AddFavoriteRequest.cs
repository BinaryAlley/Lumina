#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Enums.Favorites;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.Favorites;

/// <summary>
/// Represents a request for marking a media item as a favorite of the current user.
/// </summary>
/// <param name="MediaKind">The kind of the media item to favorite. Required.</param>
/// <param name="MediaItemId">The Id of the media item to favorite. Required.</param>
[DebuggerDisplay("MediaKind: {MediaKind}; MediaItemId: {MediaItemId}")]
public record AddFavoriteRequest(
    FavoriteMediaKind? MediaKind,
    Guid? MediaItemId
);
