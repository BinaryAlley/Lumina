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
        // Validates the identifier of the media library that owns the album, taken from the route.
        RuleFor(command => command.LibraryId)
            .NotEmpty()
            .WithError(Errors.Library.LibraryIdCannotBeEmpty);

        RuleFor(command => command.LibraryId)
            .Must(libraryId => Guid.TryParse(libraryId, out Guid parsedLibraryId) && parsedLibraryId != Guid.Empty)
            .When(command => command.LibraryId is not null && command.LibraryId.Length > 0)
            .WithError(Errors.Library.LibraryIdCannotBeEmpty);

        // Validates the identifier of the artist the album belongs to, taken from the route.
        RuleFor(command => command.ArtistId)
            .NotEmpty()
            .WithError(Errors.Music.ArtistIdCannotBeEmpty);

        RuleFor(command => command.ArtistId)
            .Must(artistId => Guid.TryParse(artistId, out Guid parsedArtistId) && parsedArtistId != Guid.Empty)
            .When(command => command.ArtistId is not null && command.ArtistId.Length > 0)
            .WithError(Errors.Music.ArtistIdCannotBeEmpty);

        // Validates the identifier of the album to delete, taken from the route.
        RuleFor(command => command.AlbumId)
            .NotEmpty()
            .WithError(Errors.Music.AlbumIdCannotBeEmpty);

        RuleFor(command => command.AlbumId)
            .Must(albumId => Guid.TryParse(albumId, out Guid parsedAlbumId) && parsedAlbumId != Guid.Empty)
            .When(command => command.AlbumId is not null && command.AlbumId.Length > 0)
            .WithError(Errors.Music.AlbumIdCannotBeEmpty);
    }
}
