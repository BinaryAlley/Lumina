#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.SharedKernel.Common.Enums.Favorites;
using System;
#endregion

namespace Lumina.Application.Core.Favorites.Commands.AddFavorite;

/// <summary>
/// Validates the needed validation rules for <see cref="AddFavoriteCommand"/>.
/// </summary>
public class AddFavoriteCommandValidator : AbstractValidator<AddFavoriteCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddFavoriteCommandValidator"/> class.
    /// </summary>
    public AddFavoriteCommandValidator()
    {
        RuleFor(command => command.MediaKind)
            .NotNull()
            .WithError(Errors.Favorites.FavoriteMediaKindIsInvalid);

        RuleFor(command => command.MediaKind)
            .IsInEnum()
            .WithError(Errors.Favorites.FavoriteMediaKindIsInvalid)
            .When(command => command.MediaKind is not null);

        RuleFor(command => command.MediaKind)
            .Must(mediaKind => mediaKind != FavoriteMediaKind.None)
            .WithError(Errors.Favorites.FavoriteMediaKindIsInvalid)
            .When(command => command.MediaKind is not null);

        RuleFor(command => command.MediaItemId)
            .NotNull()
            .WithError(Errors.Favorites.FavoriteMediaItemIdCannotBeEmpty);

        RuleFor(command => command.MediaItemId)
            .NotEmpty()
            .WithError(Errors.Favorites.FavoriteMediaItemIdCannotBeEmpty)
            .When(command => command.MediaItemId is not null);
    }
}
