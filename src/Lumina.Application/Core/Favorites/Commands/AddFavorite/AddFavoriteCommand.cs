#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Domain.SharedKernel.Common.Enums.Favorites;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.Favorites.Commands.AddFavorite;

/// <summary>
/// Command for marking a media item as a favorite of the current user.
/// </summary>
/// <param name="MediaKind">The kind of the media item to favorite.</param>
/// <param name="MediaItemId">The unique identifier of the media item to favorite.</param>
[DebuggerDisplay("MediaKind: {MediaKind}; MediaItemId: {MediaItemId}")]
public record AddFavoriteCommand(
    FavoriteMediaKind? MediaKind,
    Guid? MediaItemId
) : ICommand;
