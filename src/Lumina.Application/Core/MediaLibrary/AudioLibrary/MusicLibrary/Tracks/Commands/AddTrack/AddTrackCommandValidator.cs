#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.AddTrack;

/// <summary>
/// Validates the needed validation rules for <see cref="AddTrackCommand"/>.
/// </summary>
public class AddTrackCommandValidator : AbstractValidator<AddTrackCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddTrackCommandValidator"/> class.
    /// </summary>
    public AddTrackCommandValidator()
    {
        // Validates the identifier of the media library that owns the track, taken from the route.
        RuleFor(command => command.LibraryId)
            .NotEmpty()
            .WithError(Errors.Library.LibraryIdCannotBeEmpty);

        RuleFor(command => command.LibraryId)
            .Must(libraryId => Guid.TryParse(libraryId, out Guid parsedLibraryId) && parsedLibraryId != Guid.Empty)
            .When(command => command.LibraryId is not null && command.LibraryId.Length > 0)
            .WithError(Errors.Library.LibraryIdCannotBeEmpty);

        // Validates the identifier of the artist that owns the track, taken from the route.
        RuleFor(command => command.ArtistId)
            .NotEmpty()
            .WithError(Errors.Music.ArtistIdCannotBeEmpty);

        RuleFor(command => command.ArtistId)
            .Must(artistId => Guid.TryParse(artistId, out Guid parsedArtistId) && parsedArtistId != Guid.Empty)
            .When(command => command.ArtistId is not null && command.ArtistId.Length > 0)
            .WithError(Errors.Music.ArtistIdCannotBeEmpty);

        // Validates the identifier of the album the track is added to, taken from the route.
        RuleFor(command => command.AlbumId)
            .NotEmpty()
            .WithError(Errors.Music.AlbumIdCannotBeEmpty);

        RuleFor(command => command.AlbumId)
            .Must(albumId => Guid.TryParse(albumId, out Guid parsedAlbumId) && parsedAlbumId != Guid.Empty)
            .When(command => command.AlbumId is not null && command.AlbumId.Length > 0)
            .WithError(Errors.Music.AlbumIdCannotBeEmpty);

        RuleFor(command => command.Path)
            .NotEmpty()
            .WithError(Errors.Music.TrackPathCannotBeEmpty)
            .MaximumLength(2048)
            .WithError(Errors.Music.TrackPathMustBeMaximum2048CharactersLong);

        // Validates the audio metadata of the track: title, lengths, release information, languages, genres and tags.
        RuleFor(command => command.Metadata)
            .NotNull()
            .WithError(Errors.Metadata.MetadataCannotBeNull)
            .ChildRules(trackMetadata =>
            {
                trackMetadata.RuleFor(m => m!.Title)
                    .NotNull()
                    .NotEmpty()
                    .WithError(Errors.Metadata.TitleCannotBeEmpty)
                    .MaximumLength(255)
                    .WithError(Errors.Metadata.TitleMustBeMaximum255CharactersLong);

                trackMetadata.RuleFor(m => m!.OriginalTitle)
                    .MaximumLength(255)
                    .When(m => m!.OriginalTitle is not null)
                    .WithError(Errors.Metadata.OriginalTitleMustBeMaximum255CharactersLong);

                trackMetadata.RuleFor(m => m!.Description)
                    .MaximumLength(2000)
                    .When(m => m!.Description is not null)
                    .WithError(Errors.Metadata.DescriptionMustBeMaximum2000CharactersLong);

                trackMetadata.RuleFor(m => m!.Disambiguation)
                    .MaximumLength(255)
                    .When(m => m!.Disambiguation is not null)
                    .WithError(Errors.Metadata.DescriptionMustBeMaximum2000CharactersLong);

                trackMetadata.RuleFor(m => m!.ReleaseInfo)
                    .NotNull()
                    .WithError(Errors.Metadata.ReleaseInfoCannotBeNull)
                    .ChildRules(releaseInfo =>
                    {
                        releaseInfo.RuleFor(r => r!.OriginalReleaseYear)
                            .InclusiveBetween(1, 9999)
                            .When(r => r!.OriginalReleaseYear.HasValue)
                            .WithError(Errors.Metadata.OriginalReleaseYearMustBeBetween1And9999);

                        releaseInfo.RuleFor(r => r!.ReReleaseYear)
                            .InclusiveBetween(1, 9999)
                            .When(r => r!.ReReleaseYear.HasValue)
                            .WithError(Errors.Metadata.ReReleaseYearMustBeBetween1And9999);

                        releaseInfo.RuleFor(r => r!.ReleaseVersion)
                            .MaximumLength(50)
                            .When(r => r!.ReleaseVersion is not null)
                            .WithError(Errors.Metadata.ReleaseVersionMustBeMaximum50CharactersLong);

                        releaseInfo.RuleFor(r => r!.OriginalReleaseYear)
                            .Must((releaseInfoInstance, originalReleaseYear) =>
                                !releaseInfoInstance!.OriginalReleaseDate.HasValue ||
                                !releaseInfoInstance.OriginalReleaseYear.HasValue ||
                                originalReleaseYear == releaseInfoInstance.OriginalReleaseDate.Value.Year)
                            .When(r => r!.OriginalReleaseDate.HasValue && r.OriginalReleaseYear.HasValue)
                            .WithError(Errors.Metadata.OriginalReleaseDateAndYearMustMatch);

                        releaseInfo.RuleFor(r => r!.ReReleaseYear)
                            .Must((releaseInfoInstance, reReleaseYear) =>
                                !releaseInfoInstance!.ReReleaseDate.HasValue ||
                                !releaseInfoInstance.ReReleaseYear.HasValue ||
                                reReleaseYear == releaseInfoInstance.ReReleaseDate.Value.Year)
                            .When(r => r!.ReReleaseDate.HasValue && r.ReReleaseYear.HasValue)
                            .WithError(Errors.Metadata.ReReleaseDateAndYearMustMatch);

                        releaseInfo.RuleFor(r => r!.ReReleaseYear)
                            .Must((releaseInfoInstance, reReleaseYear) =>
                                !releaseInfoInstance!.ReReleaseYear.HasValue ||
                                !releaseInfoInstance.OriginalReleaseYear.HasValue ||
                                reReleaseYear >= releaseInfoInstance.OriginalReleaseYear)
                            .When(r => r!.ReReleaseYear.HasValue && r.OriginalReleaseYear.HasValue)
                            .WithError(Errors.Metadata.ReReleaseYearCannotBeEarlierThanOriginalReleaseYear);

                        releaseInfo.RuleFor(r => r!.ReReleaseDate)
                            .Must((releaseInfoInstance, reReleaseDate) =>
                                !releaseInfoInstance!.ReReleaseDate.HasValue ||
                                !releaseInfoInstance.OriginalReleaseDate.HasValue ||
                                reReleaseDate >= releaseInfoInstance.OriginalReleaseDate)
                            .When(r => r!.ReReleaseDate.HasValue && r.OriginalReleaseDate.HasValue)
                            .WithError(Errors.Metadata.ReReleaseDateCannotBeEarlierThanOriginalReleaseDate);
                    });

                trackMetadata.RuleFor(m => m!.Genres)
                    .NotNull()
                    .WithError(Errors.Metadata.GenresListCannotBeNull);

                trackMetadata.RuleForEach(m => m!.Genres)
                    .ChildRules(genre =>
                        genre.RuleFor(g => g.Name)
                            .NotEmpty()
                            .WithError(Errors.Metadata.GenreNameCannotBeEmpty)
                            .MaximumLength(50)
                            .WithError(Errors.Metadata.GenreNameMustBeMaximum50CharactersLong));

                trackMetadata.RuleFor(m => m!.Tags)
                    .NotNull()
                    .WithError(Errors.Metadata.TagsListCannotBeNull);

                trackMetadata.RuleForEach(m => m!.Tags)
                    .ChildRules(tag =>
                        tag.RuleFor(t => t.Name)
                            .NotEmpty()
                            .WithError(Errors.Metadata.TagNameCannotBeEmpty)
                            .MaximumLength(50)
                            .WithError(Errors.Metadata.TagNameMustBeMaximum50CharactersLong));

                trackMetadata.RuleFor(m => m!.Language!.LanguageCode)
                    .NotEmpty()
                    .WithError(Errors.Metadata.LanguageCodeCannotBeEmpty)
                    .Length(2)
                    .WithError(Errors.Metadata.LanguageCodeMustBe2CharactersLong)
                    .When(m => m!.Language is not null);

                trackMetadata.RuleFor(m => m!.Language!.LanguageName)
                    .NotEmpty()
                    .WithError(Errors.Metadata.LanguageNameCannotBeEmpty)
                    .MaximumLength(50)
                    .WithError(Errors.Metadata.LanguageNameMustBeMaximum50CharactersLong)
                    .When(m => m!.Language is not null);

                trackMetadata.RuleFor(m => m!.Language!.NativeName)
                    .MaximumLength(50)
                    .WithError(Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong)
                    .When(m => m!.Language is not null);

                trackMetadata.RuleFor(m => m!.OriginalLanguage!.LanguageCode)
                    .NotEmpty()
                    .WithError(Errors.Metadata.LanguageCodeCannotBeEmpty)
                    .Length(2)
                    .WithError(Errors.Metadata.LanguageCodeMustBe2CharactersLong)
                    .When(m => m!.OriginalLanguage is not null);

                trackMetadata.RuleFor(m => m!.OriginalLanguage!.LanguageName)
                    .NotEmpty()
                    .WithError(Errors.Metadata.LanguageNameCannotBeEmpty)
                    .MaximumLength(50)
                    .WithError(Errors.Metadata.LanguageNameMustBeMaximum50CharactersLong)
                    .When(m => m!.OriginalLanguage is not null);

                trackMetadata.RuleFor(m => m!.OriginalLanguage!.NativeName)
                    .MaximumLength(50)
                    .WithError(Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong)
                    .When(m => m!.OriginalLanguage is not null);
            });

        // Validates the MusicBrainz identifiers of the track.
        RuleFor(command => command.MusicBrainzRecordingId)
            .Must(musicBrainzRecordingId => musicBrainzRecordingId != Guid.Empty)
            .When(command => command.MusicBrainzRecordingId.HasValue)
            .WithError(Errors.Music.MusicBrainzIdInvalidFormat);

        RuleFor(command => command.MusicBrainzTrackId)
            .Must(musicBrainzTrackId => musicBrainzTrackId != Guid.Empty)
            .When(command => command.MusicBrainzTrackId.HasValue)
            .WithError(Errors.Music.MusicBrainzIdInvalidFormat);

        // Validates the ordering and performance characteristics of the track.
        RuleFor(command => command.TrackNumber)
            .NotNull()
            .WithError(Errors.Music.TrackNumberMustBeGreaterThanZero);

        RuleFor(command => command.TrackNumber)
            .GreaterThan(0)
            .When(command => command.TrackNumber.HasValue)
            .WithError(Errors.Music.TrackNumberMustBeGreaterThanZero);

        RuleFor(command => command.DiscNumber)
            .GreaterThan(0)
            .When(command => command.DiscNumber.HasValue)
            .WithError(Errors.Music.DiscNumberMustBeGreaterThanZero);

        RuleFor(command => command.Script)
            .MaximumLength(50)
            .When(command => command.Script is not null)
            .WithError(Errors.Music.ScriptMustBeMaximum50CharactersLong);

        RuleFor(command => command.Key)
            .IsInEnum()
            .When(command => command.Key is not null)
            .WithError(Errors.Music.UnknownMusicKey);

        RuleFor(command => command.Bpm)
            .GreaterThan(0)
            .When(command => command.Bpm.HasValue)
            .WithError(Errors.Music.BpmMustBeGreaterThanZero);

        // Validates the work the track is a recording of.
        RuleFor(command => command.Work!.Title)
            .MaximumLength(255)
            .When(command => command.Work is not null && command.Work.Title is not null)
            .WithError(Errors.Music.WorkMustBeMaximum255CharactersLong);

        RuleFor(command => command.Work!.MusicBrainzWorkId)
            .Must(musicBrainzWorkId => musicBrainzWorkId is null || musicBrainzWorkId != Guid.Empty)
            .When(command => command.Work is not null)
            .WithError(Errors.Music.MusicBrainzIdInvalidFormat);

        // Validates the moods and the ISRC codes of the track.
        RuleForEach(command => command.Moods)
            .ChildRules(mood =>
                mood.RuleFor(m => m.Name)
                    .NotEmpty()
                    .WithError(Errors.Metadata.MoodNameCannotBeEmpty));

        RuleForEach(command => command.Isrcs)
            .ChildRules(isrc =>
                isrc.RuleFor(i => i.Value)
                    .NotEmpty()
                    .WithError(Errors.Music.IsrcValueCannotBeEmpty));

        // Validates the media contributors that performed on the track.
        RuleFor(command => command.Contributors)
            .NotNull()
            .WithError(Errors.MediaContributor.ContributorsListCannotBeNull);

        RuleForEach(command => command.Contributors)
            .ChildRules(contributor =>
            {
                contributor.RuleFor(c => c.ContributorId)
                    .NotEmpty()
                    .WithError(Errors.MediaContributor.MediaContributorIdCannotBeEmpty)
                    .Must(contributorId => contributorId != Guid.Empty)
                    .WithError(Errors.MediaContributor.MediaContributorIdCannotBeEmpty);

                contributor.RuleFor(c => c.Role)
                    .IsInEnum()
                    .WithError(Errors.MediaContributor.UnknownMediaContributorRole);
            });

        // Validates the ratings of the track.
        RuleFor(command => command.Ratings)
            .NotNull()
            .WithError(Errors.Metadata.RatingsListCannotBeNull);

        RuleForEach(command => command.Ratings)
            .ChildRules(rating =>
            {
                rating.RuleFor(r => r.Value)
                    .GreaterThan(0)
                    .WithError(Errors.Metadata.RatingValueMustBePositive)
                    .Must((ratingInstance, value) => value <= ratingInstance.MaxValue)
                    .WithError(Errors.Metadata.RatingValueCannotBeGreaterThanMaxValue);

                rating.RuleFor(r => r.MaxValue)
                    .GreaterThan(0)
                    .WithError(Errors.Metadata.RatingMaxValueMustBePositive);

                rating.RuleFor(r => r.VoteCount)
                    .GreaterThanOrEqualTo(0)
                    .When(r => r.VoteCount.HasValue)
                    .WithError(Errors.Metadata.RatingVoteCountMustBePositive);
            });
    }
}
