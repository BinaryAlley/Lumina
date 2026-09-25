#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaContributors;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Commands.AddArtist;
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
/// Extension methods for converting <see cref="AddArtistCommand"/>.
/// </summary>
public static class AddArtistCommandMapping
{
    /// <summary>
    /// Converts <paramref name="command"/> to <see cref="Artist"/>.
    /// </summary>
    /// <param name="command">The command to be converted.</param>
    /// <param name="libraryId">The Id of the media library the artist is added to.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully converted <see cref="Artist"/>, or an error message.
    /// </returns>
    public static Result<Artist> ToDomainEntity(this AddArtistCommand command, Guid libraryId)
    {
        // Map the media contributors referenced by the user to their domain counterparts; a contributor is identified by its id and carries the role it played.
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
            musicBrainzArtistId = MusicBrainzId.Create(command.MusicBrainzArtistId.Value);

        return Artist.Create(
            LibraryId.Create(libraryId),
            command.Name!,
            website,
            musicBrainzArtistId,
            [.. domainContributorsResult.Select(contributorResult => contributorResult.Value)],
            [.. domainAlbumsResult.Select(albumResult => albumResult.Value)]);
    }
}
