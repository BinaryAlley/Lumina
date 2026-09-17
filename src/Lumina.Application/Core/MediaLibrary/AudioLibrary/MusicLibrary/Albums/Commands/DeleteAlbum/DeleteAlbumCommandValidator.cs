#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Commands.DeleteAlbum;

/// <summary>
/// Validates the needed validation rules for <see cref="DeleteAlbumCommand"/>.
/// </summary>
public class DeleteAlbumCommandValidator : AbstractValidator<DeleteAlbumCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteAlbumCommandValidator"/> class.
    /// </summary>
    public DeleteAlbumCommandValidator()
    {
        RuleFor(command => command.AlbumId)
            .NotEmpty()
            .WithError(Errors.Music.AlbumIdCannotBeEmpty)
            .Must(albumId => albumId != Guid.Empty)
            .WithError(Errors.Music.AlbumIdCannotBeEmpty);
    }
}
