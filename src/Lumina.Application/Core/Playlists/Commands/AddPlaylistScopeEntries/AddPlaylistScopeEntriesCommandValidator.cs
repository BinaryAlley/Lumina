#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.Playlists.Commands.AddPlaylistScopeEntries;

/// <summary>
/// Validates the needed validation rules for <see cref="AddPlaylistScopeEntriesCommand"/>.
/// </summary>
public class AddPlaylistScopeEntriesCommandValidator : AbstractValidator<AddPlaylistScopeEntriesCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddPlaylistScopeEntriesCommandValidator"/> class.
    /// </summary>
    public AddPlaylistScopeEntriesCommandValidator()
    {
        RuleFor(command => command.PlaylistId)
            .NotEmpty()
            .WithError(Errors.Playlists.PlaylistIdCannotBeEmpty);

        RuleFor(command => command.PlaylistId)
            .Must(playlistId => Guid.TryParse(playlistId, out Guid parsedPlaylistId) && parsedPlaylistId != Guid.Empty)
            .When(command => command.PlaylistId is not null && command.PlaylistId.Length > 0)
            .WithError(Errors.Playlists.PlaylistIdCannotBeEmpty);

        RuleFor(command => command.LibraryId)
            .Must(libraryId => libraryId.HasValue && libraryId.Value != Guid.Empty)
            .WithError(Errors.Library.LibraryIdCannotBeEmpty);

        RuleFor(command => command.ArtistId)
            .Must(artistId => !artistId.HasValue || artistId.Value != Guid.Empty)
            .WithError(Errors.Music.ArtistIdCannotBeEmpty);

        RuleFor(command => command.AlbumId)
            .Must(albumId => !albumId.HasValue || albumId.Value != Guid.Empty)
            .WithError(Errors.Music.AlbumIdCannotBeEmpty);
    }
}
