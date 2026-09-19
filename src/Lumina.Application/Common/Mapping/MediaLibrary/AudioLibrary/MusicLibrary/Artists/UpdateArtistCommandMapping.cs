#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.Mapping.MediaContributors;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Commands.UpdateArtist;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.ExternalIdentifiers.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Artists;

/// <summary>
/// Extension methods for converting <see cref="UpdateArtistCommand"/>.
/// </summary>
public static class UpdateArtistCommandMapping
{
    /// <summary>
    /// Converts <paramref name="command"/> to <see cref="Artist"/>, preserving the identity and creation metadata of the stored artist identified by <paramref name="existingArtist"/>.
    /// </summary>
    /// <param name="command">The command to be converted.</param>
    /// <param name="existingArtist">The stored artist whose identity and creation metadata are preserved.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully converted <see cref="Artist"/>, or an error message.
    /// </returns>
    public static Result<Artist> ToDomainEntity(this UpdateArtistCommand command, ArtistEntity existingArtist)
    {
        // Map the media contributors that make up the artist to their domain counterparts.
        IEnumerable<Result<MusicMediaContributor>> domainContributorsResult = command.Contributors!.ToMusicDomainEntities();
        List<Error> errors = [.. domainContributorsResult.Where(contributorResult => contributorResult.IsFailure).SelectMany(contributorResult => contributorResult.Errors)];
        if (errors.Count > 0)
            return errors;

        // Build each album of the artist, together with its metadata, contributors, ratings and tracks.
        IEnumerable<Result<Album>> domainAlbumsResult = command.Albums!.ToDomainEntities();
        errors = [.. domainAlbumsResult.Where(albumResult => albumResult.IsFailure).SelectMany(albumResult => albumResult.Errors)];
        if (errors.Count > 0)
            return errors;

        Optional<string> website = Optional<string>.FromNullable(command.Website);
        Optional<MusicBrainzId> musicBrainzArtistId = Optional<MusicBrainzId>.None();
        if (command.MusicBrainzArtistId is not null)
        {
            Result<MusicBrainzId> musicBrainzArtistIdResult = MusicBrainzId.Create(command.MusicBrainzArtistId.Value);
            if (musicBrainzArtistIdResult.IsFailure)
                return musicBrainzArtistIdResult.Errors;
            musicBrainzArtistId = musicBrainzArtistIdResult.Value;
        }

        return Artist.Create(
            ArtistId.Create(existingArtist.Id),
            LibraryId.Create(existingArtist.LibraryId),
            command.Name!,
            website,
            musicBrainzArtistId,
            [.. domainContributorsResult.Select(contributorResult => contributorResult.Value)],
            [.. domainAlbumsResult.Select(albumResult => albumResult.Value)],
            existingArtist.CreatedOnUtc,
            Optional<DateTime>.FromNullable(existingArtist.UpdatedOnUtc));
    }
}
