#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Commands.UpdateArtist;

/// <summary>
/// Validates the needed validation rules for <see cref="UpdateArtistCommand"/>.
/// </summary>
public class UpdateArtistCommandValidator : AbstractValidator<UpdateArtistCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateArtistCommandValidator"/> class.
    /// </summary>
    public UpdateArtistCommandValidator()
    {
        // Validates the identifier of the media library that owns the artist, taken from the route.
        RuleFor(command => command.LibraryId)
            .NotEmpty()
            .WithError(Errors.Library.LibraryIdCannotBeEmpty);

        RuleFor(command => command.LibraryId)
            .Must(libraryId => Guid.TryParse(libraryId, out Guid parsedLibraryId) && parsedLibraryId != Guid.Empty)
            .When(command => command.LibraryId is not null && command.LibraryId.Length > 0)
            .WithError(Errors.Library.LibraryIdCannotBeEmpty);

        // Validates the identifier of the artist, taken from the route.
        RuleFor(command => command.ArtistId)
            .NotEmpty()
            .WithError(Errors.Music.ArtistIdCannotBeEmpty);

        RuleFor(command => command.ArtistId)
            .Must(artistId => Guid.TryParse(artistId, out Guid parsedArtistId) && parsedArtistId != Guid.Empty)
            .When(command => command.ArtistId is not null && command.ArtistId.Length > 0)
            .WithError(Errors.Music.ArtistIdCannotBeEmpty);

        // Validates the identifying details of the artist.
        RuleFor(command => command.Name)
            .NotNull()
            .NotEmpty()
            .WithError(Errors.Music.ArtistNameCannotBeEmpty)
            .MaximumLength(255)
            .WithError(Errors.Music.ArtistNameMustBeMaximum255CharactersLong);

        RuleFor(command => command.Website)
            .MaximumLength(2048)
            .When(command => command.Website is not null)
            .WithError(Errors.Music.ArtistWebsiteMustBeMaximum2048CharactersLong);

        RuleFor(command => command.MusicBrainzArtistId)
            .Must(musicBrainzArtistId => musicBrainzArtistId != Guid.Empty)
            .When(command => command.MusicBrainzArtistId.HasValue)
            .WithError(Errors.Music.MusicBrainzIdInvalidFormat);

        // Validates the media contributors that make up the artist.
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

        // Validates each album updated together with the artist, when the request carries albums.
        RuleFor(command => command.Albums)
            .NotNull()
            .WithError(Errors.Music.AlbumsListCannotBeNull);

        RuleForEach(command => command.Albums)
            .ChildRules(album =>
            {
                // Validates the metadata of the album: title, lengths, release type and status, and disc and track counts.
                album.RuleFor(a => a!.Metadata)
                    .NotNull()
                    .WithError(Errors.Metadata.MetadataCannotBeNull)
                    .ChildRules(metadata =>
                    {
                        metadata.RuleFor(m => m!.Title)
                            .NotNull()
                            .NotEmpty()
                            .WithError(Errors.Music.AlbumTitleCannotBeEmpty)
                            .MaximumLength(255)
                            .WithError(Errors.Music.AlbumTitleMustBeMaximum255CharactersLong);

                        metadata.RuleFor(m => m!.OriginalTitle)
                            .MaximumLength(255)
                            .When(m => m!.OriginalTitle is not null)
                            .WithError(Errors.Metadata.OriginalTitleMustBeMaximum255CharactersLong);

                        metadata.RuleFor(m => m!.Description)
                            .MaximumLength(2000)
                            .When(m => m!.Description is not null)
                            .WithError(Errors.Metadata.DescriptionMustBeMaximum2000CharactersLong);

                        metadata.RuleFor(m => m!.ReleaseType)
                            .IsInEnum()
                            .When(m => m!.ReleaseType is not null)
                            .WithError(Errors.Music.UnknownMusicReleaseType);

                        metadata.RuleFor(m => m!.ReleaseStatus)
                            .IsInEnum()
                            .When(m => m!.ReleaseStatus is not null)
                            .WithError(Errors.Music.UnknownMusicReleaseStatus);

                        metadata.RuleFor(m => m!.TotalDiscs)
                            .GreaterThan(0)
                            .When(m => m!.TotalDiscs.HasValue)
                            .WithError(Errors.Music.TotalDiscsMustBeGreaterThanZero);

                        metadata.RuleFor(m => m!.TotalTracks)
                            .GreaterThan(0)
                            .When(m => m!.TotalTracks.HasValue)
                            .WithError(Errors.Music.TotalTracksMustBeGreaterThanZero);

                        metadata.RuleFor(m => m!.ReleaseInfo)
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

                        metadata.RuleFor(m => m!.Genres)
                            .NotNull()
                            .WithError(Errors.Metadata.GenresListCannotBeNull);

                        metadata.RuleForEach(m => m!.Genres)
                            .ChildRules(genre =>
                                genre.RuleFor(g => g.Name)
                                    .NotEmpty()
                                    .WithError(Errors.Metadata.GenreNameCannotBeEmpty)
                                    .MaximumLength(50)
                                    .WithError(Errors.Metadata.GenreNameMustBeMaximum50CharactersLong));

                        metadata.RuleFor(m => m!.Tags)
                            .NotNull()
                            .WithError(Errors.Metadata.TagsListCannotBeNull);

                        metadata.RuleForEach(m => m!.Tags)
                            .ChildRules(tag =>
                                tag.RuleFor(t => t.Name)
                                    .NotEmpty()
                                    .WithError(Errors.Metadata.TagNameCannotBeEmpty)
                                    .MaximumLength(50)
                                    .WithError(Errors.Metadata.TagNameMustBeMaximum50CharactersLong));

                        metadata.RuleFor(m => m!.Language!.LanguageCode)
                            .NotEmpty()
                            .WithError(Errors.Metadata.LanguageCodeCannotBeEmpty)
                            .Length(2)
                            .WithError(Errors.Metadata.LanguageCodeMustBe2CharactersLong)
                            .When(m => m!.Language is not null);

                        metadata.RuleFor(m => m!.Language!.LanguageName)
                            .NotEmpty()
                            .WithError(Errors.Metadata.LanguageNameCannotBeEmpty)
                            .MaximumLength(50)
                            .WithError(Errors.Metadata.LanguageNameMustBeMaximum50CharactersLong)
                            .When(m => m!.Language is not null);

                        metadata.RuleFor(m => m!.Language!.NativeName)
                            .MaximumLength(50)
                            .WithError(Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong)
                            .When(m => m!.Language is not null);

                        metadata.RuleFor(m => m!.OriginalLanguage!.LanguageCode)
                            .NotEmpty()
                            .WithError(Errors.Metadata.LanguageCodeCannotBeEmpty)
                            .Length(2)
                            .WithError(Errors.Metadata.LanguageCodeMustBe2CharactersLong)
                            .When(m => m!.OriginalLanguage is not null);

                        metadata.RuleFor(m => m!.OriginalLanguage!.LanguageName)
                            .NotEmpty()
                            .WithError(Errors.Metadata.LanguageNameCannotBeEmpty)
                            .MaximumLength(50)
                            .WithError(Errors.Metadata.LanguageNameMustBeMaximum50CharactersLong)
                            .When(m => m!.OriginalLanguage is not null);

                        metadata.RuleFor(m => m!.OriginalLanguage!.NativeName)
                            .MaximumLength(50)
                            .WithError(Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong)
                            .When(m => m!.OriginalLanguage is not null);
                    });

                // Validates the physical characteristics of the album: format and catalog number.
                album.RuleFor(a => a!.MediaFormat)
                    .IsInEnum()
                    .When(a => a!.MediaFormat is not null)
                    .WithError(Errors.Music.UnknownMusicMediaFormat);

                album.RuleFor(a => a!.CatalogNumber)
                    .MaximumLength(50)
                    .When(a => a!.CatalogNumber is not null)
                    .WithError(Errors.Music.CatalogNumberMustBeMaximum50CharactersLong);

                // Validates the MusicBrainz identifiers of the album.
                album.RuleFor(a => a!.MusicBrainzReleaseId)
                    .Must(musicBrainzReleaseId => musicBrainzReleaseId != Guid.Empty)
                    .When(a => a!.MusicBrainzReleaseId.HasValue)
                    .WithError(Errors.Music.MusicBrainzIdInvalidFormat);

                album.RuleFor(a => a!.MusicBrainzReleaseGroupId)
                    .Must(musicBrainzReleaseGroupId => musicBrainzReleaseGroupId != Guid.Empty)
                    .When(a => a!.MusicBrainzReleaseGroupId.HasValue)
                    .WithError(Errors.Music.MusicBrainzIdInvalidFormat);

                album.RuleFor(a => a!.MusicBrainzReleaseArtistId)
                    .Must(musicBrainzReleaseArtistId => musicBrainzReleaseArtistId != Guid.Empty)
                    .When(a => a!.MusicBrainzReleaseArtistId.HasValue)
                    .WithError(Errors.Music.MusicBrainzIdInvalidFormat);

                // Validates the media contributors that performed on the album.
                album.RuleFor(a => a!.Contributors)
                    .NotNull()
                    .WithError(Errors.MediaContributor.ContributorsListCannotBeNull);

                album.RuleForEach(a => a!.Contributors)
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

                // Validates the ratings of the album.
                album.RuleFor(a => a!.Ratings)
                    .NotNull()
                    .WithError(Errors.Metadata.RatingsListCannotBeNull);

                album.RuleForEach(a => a!.Ratings)
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

                // Validates the tracks of the album.
                album.RuleFor(a => a!.Tracks)
                    .NotNull()
                    .WithError(Errors.Music.TracksListCannotBeNull);

                album.RuleForEach(a => a!.Tracks)
                    .ChildRules(track =>
                    {
                        // Validates the file system path of the track.
                        track.RuleFor(t => t!.Path)
                            .NotEmpty()
                            .WithError(Errors.Music.TrackPathCannotBeEmpty)
                            .MaximumLength(2048)
                            .WithError(Errors.Music.TrackPathMustBeMaximum2048CharactersLong);

                        // Validates the audio metadata of the track: title, lengths, release information, languages, genres and tags.
                        track.RuleFor(t => t!.Metadata)
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

                        // Validates the ordering and performance characteristics of the track.
                        track.RuleFor(t => t!.TrackNumber)
                            .GreaterThan(0)
                            .When(t => t!.TrackNumber.HasValue)
                            .WithError(Errors.Music.TrackNumberMustBeGreaterThanZero);

                        track.RuleFor(t => t!.DiscNumber)
                            .GreaterThan(0)
                            .When(t => t!.DiscNumber.HasValue)
                            .WithError(Errors.Music.DiscNumberMustBeGreaterThanZero);

                        track.RuleFor(t => t!.Script)
                            .MaximumLength(50)
                            .When(t => t!.Script is not null)
                            .WithError(Errors.Music.ScriptMustBeMaximum50CharactersLong);

                        track.RuleFor(t => t!.Key)
                            .IsInEnum()
                            .When(t => t!.Key is not null)
                            .WithError(Errors.Music.UnknownMusicKey);

                        track.RuleFor(t => t!.Bpm)
                            .GreaterThan(0)
                            .When(t => t!.Bpm.HasValue)
                            .WithError(Errors.Music.BpmMustBeGreaterThanZero);

                        track.RuleFor(t => t!.Work)
                            .MaximumLength(255)
                            .When(t => t!.Work is not null)
                            .WithError(Errors.Music.WorkMustBeMaximum255CharactersLong);

                        // Validates the media contributors that performed on the track.
                        track.RuleFor(t => t!.Contributors)
                            .NotNull()
                            .WithError(Errors.MediaContributor.ContributorsListCannotBeNull);

                        track.RuleForEach(t => t!.Contributors)
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

                        // Validates the moods and the ISRC codes of the track.
                        track.RuleForEach(t => t!.Moods)
                            .ChildRules(mood =>
                                mood.RuleFor(m => m.Name)
                                    .NotEmpty()
                                    .WithError(Errors.Metadata.MoodNameCannotBeEmpty));

                        track.RuleForEach(t => t!.Isrcs)
                            .ChildRules(isrc =>
                                isrc.RuleFor(i => i.Value)
                                    .NotEmpty()
                                    .WithError(Errors.Music.IsrcValueCannotBeEmpty));

                        // Validates the ratings of the track.
                        track.RuleFor(t => t!.Ratings)
                            .NotNull()
                            .WithError(Errors.Metadata.RatingsListCannotBeNull);

                        track.RuleForEach(t => t!.Ratings)
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
                    });
            });
    }
}
