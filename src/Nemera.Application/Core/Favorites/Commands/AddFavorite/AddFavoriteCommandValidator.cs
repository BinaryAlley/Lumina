#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Application.Common.Utilities;
using Nemera.Domain.Common.Errors;
using Nemera.Domain.SharedKernel.Common.Enums.Favorites;
using System;
#endregion

namespace Nemera.Application.Core.Favorites.Commands.AddFavorite;

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
