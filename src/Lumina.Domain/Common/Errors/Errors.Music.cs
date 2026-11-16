#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
#endregion

namespace Lumina.Domain.Common.Errors;

/// <summary>
/// Music error types.
/// </summary>
public static partial class Errors
{
    /// <summary>
    /// Defines the errors that report failures raised while working with music artists, albums and tracks.
    /// </summary>
    public static class Music
    {
        /// <summary>
        /// Gets the error that reports that the requested music artist does not exist.
        /// </summary>
        public static Error ArtistNotFound => Error.NotFound(description: nameof(ArtistNotFound));

        /// <summary>
        /// Gets the error that reports that a music artist with the same identifier or name already exists in the library.
        /// </summary>
        public static Error ArtistAlreadyExists => Error.Conflict(description: nameof(ArtistAlreadyExists));

        /// <summary>
        /// Gets the error that reports that a music artist identifier cannot be empty.
        /// </summary>
        public static Error ArtistIdCannotBeEmpty => Error.Validation(description: nameof(ArtistIdCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that a music artist name cannot be empty.
        /// </summary>
        public static Error ArtistNameCannotBeEmpty => Error.Validation(description: nameof(ArtistNameCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that a music artist name exceeds the maximum length of 255 characters.
        /// </summary>
        public static Error ArtistNameMustBeMaximum255CharactersLong => Error.Validation(description: nameof(ArtistNameMustBeMaximum255CharactersLong));

        /// <summary>
        /// Gets the error that reports that a music artist website exceeds the maximum length of 2048 characters.
        /// </summary>
        public static Error ArtistWebsiteMustBeMaximum2048CharactersLong => Error.Validation(description: nameof(ArtistWebsiteMustBeMaximum2048CharactersLong));

        /// <summary>
        /// Gets the error that reports that a MusicBrainz identifier does not have a valid format.
        /// </summary>
        public static Error MusicBrainzIdInvalidFormat => Error.Validation(description: nameof(MusicBrainzIdInvalidFormat));

        /// <summary>
        /// Gets the error that reports that an ISRC value cannot be empty.
        /// </summary>
        public static Error IsrcValueCannotBeEmpty => Error.Validation(description: nameof(IsrcValueCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that an ISRC value does not have a valid ISRC format.
        /// </summary>
        public static Error IsrcInvalidFormat => Error.Validation(description: nameof(IsrcInvalidFormat));

        /// <summary>
        /// Gets the error that reports that a barcode value cannot be empty.
        /// </summary>
        public static Error BarcodeValueCannotBeEmpty => Error.Validation(description: nameof(BarcodeValueCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that a barcode is not made of 12 or 13 digits.
        /// </summary>
        public static Error InvalidFormatForBarcode => Error.Validation(description: nameof(InvalidFormatForBarcode));

        /// <summary>
        /// Gets the error that reports that the requested music album does not exist.
        /// </summary>
        public static Error AlbumNotFound => Error.NotFound(description: nameof(AlbumNotFound));

        /// <summary>
        /// Gets the error that reports that a music album with the same identifier already exists in the library.
        /// </summary>
        public static Error AlbumAlreadyExists => Error.Conflict(description: nameof(AlbumAlreadyExists));

        /// <summary>
        /// Gets the error that reports that a music artist cannot exist without at least one album.
        /// </summary>
        public static Error ArtistMustHaveAtLeastOneAlbum => Error.Forbidden(description: nameof(ArtistMustHaveAtLeastOneAlbum));

        /// <summary>
        /// Gets the error that reports that a music album identifier cannot be empty.
        /// </summary>
        public static Error AlbumIdCannotBeEmpty => Error.Validation(description: nameof(AlbumIdCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that the collection of albums for a music artist cannot be <see langword="null"/>.
        /// </summary>
        public static Error AlbumsListCannotBeNull => Error.Validation(description: nameof(AlbumsListCannotBeNull));

        /// <summary>
        /// Gets the error that reports that a music album title cannot be empty.
        /// </summary>
        public static Error AlbumTitleCannotBeEmpty => Error.Validation(description: nameof(AlbumTitleCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that a music album title exceeds the maximum length of 255 characters.
        /// </summary>
        public static Error AlbumTitleMustBeMaximum255CharactersLong => Error.Validation(description: nameof(AlbumTitleMustBeMaximum255CharactersLong));

        /// <summary>
        /// Gets the error that reports that the total number of discs in a music album must be greater than zero.
        /// </summary>
        public static Error TotalDiscsMustBeGreaterThanZero => Error.Validation(description: nameof(TotalDiscsMustBeGreaterThanZero));

        /// <summary>
        /// Gets the error that reports that the total number of tracks in a music album must be greater than zero.
        /// </summary>
        public static Error TotalTracksMustBeGreaterThanZero => Error.Validation(description: nameof(TotalTracksMustBeGreaterThanZero));

        /// <summary>
        /// Gets the error that reports that a music release type is not a recognized value.
        /// </summary>
        public static Error UnknownMusicReleaseType => Error.Validation(description: nameof(UnknownMusicReleaseType));

        /// <summary>
        /// Gets the error that reports that a music release status is not a recognized value.
        /// </summary>
        public static Error UnknownMusicReleaseStatus => Error.Validation(description: nameof(UnknownMusicReleaseStatus));

        /// <summary>
        /// Gets the error that reports that a music media format is not a recognized value.
        /// </summary>
        public static Error UnknownMusicMediaFormat => Error.Validation(description: nameof(UnknownMusicMediaFormat));

        /// <summary>
        /// Gets the error that reports that a music release packaging is not a recognized value.
        /// </summary>
        public static Error UnknownMusicReleasePackaging => Error.Validation(description: nameof(UnknownMusicReleasePackaging));

        /// <summary>
        /// Gets the error that reports that a music key is not a recognized value.
        /// </summary>
        public static Error UnknownMusicKey => Error.Validation(description: nameof(UnknownMusicKey));

        /// <summary>
        /// Gets the error that reports that a music album catalog number exceeds the maximum length of 50 characters.
        /// </summary>
        public static Error CatalogNumberMustBeMaximum50CharactersLong => Error.Validation(description: nameof(CatalogNumberMustBeMaximum50CharactersLong));

        /// <summary>
        /// Gets the error that reports that the requested music track does not exist.
        /// </summary>
        public static Error TrackNotFound => Error.NotFound(description: nameof(TrackNotFound));

        /// <summary>
        /// Gets the error that reports that the audio file of a music track does not exist or cannot be read.
        /// </summary>
        public static Error TrackFileNotFound => Error.NotFound(description: nameof(TrackFileNotFound));

        /// <summary>
        /// Gets the error that reports that a music track with the same identifier or file path already exists in the library.
        /// </summary>
        public static Error TrackAlreadyExists => Error.Conflict(description: nameof(TrackAlreadyExists));

        /// <summary>
        /// Gets the error that reports that a music track identifier cannot be empty.
        /// </summary>
        public static Error TrackIdCannotBeEmpty => Error.Validation(description: nameof(TrackIdCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that the collection of tracks for a music album cannot be <see langword="null"/>.
        /// </summary>
        public static Error TracksListCannotBeNull => Error.Validation(description: nameof(TracksListCannotBeNull));

        /// <summary>
        /// Gets the error that reports that a music track file path cannot be empty.
        /// </summary>
        public static Error TrackPathCannotBeEmpty => Error.Validation(description: nameof(TrackPathCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that a music track file path exceeds the maximum length of 2048 characters.
        /// </summary>
        public static Error TrackPathMustBeMaximum2048CharactersLong => Error.Validation(description: nameof(TrackPathMustBeMaximum2048CharactersLong));

        /// <summary>
        /// Gets the error that reports that a music track file path does not reside within any of the content locations of the library.
        /// </summary>
        public static Error TrackPathMustBeWithinLibraryContentLocations => Error.Validation(description: nameof(TrackPathMustBeWithinLibraryContentLocations));

        /// <summary>
        /// Gets the error that reports that the number of a music track must be greater than zero.
        /// </summary>
        public static Error TrackNumberMustBeGreaterThanZero => Error.Validation(description: nameof(TrackNumberMustBeGreaterThanZero));

        /// <summary>
        /// Gets the error that reports that the disc number of a music track must be greater than zero.
        /// </summary>
        public static Error DiscNumberMustBeGreaterThanZero => Error.Validation(description: nameof(DiscNumberMustBeGreaterThanZero));

        /// <summary>
        /// Gets the error that reports that a music script exceeds the maximum length of 50 characters.
        /// </summary>
        public static Error ScriptMustBeMaximum50CharactersLong => Error.Validation(description: nameof(ScriptMustBeMaximum50CharactersLong));

        /// <summary>
        /// Gets the error that reports that the title of a music work exceeds the maximum length of 255 characters.
        /// </summary>
        public static Error WorkMustBeMaximum255CharactersLong => Error.Validation(description: nameof(WorkMustBeMaximum255CharactersLong));

        /// <summary>
        /// Gets the error that reports that the beats per minute value of a music track must be greater than zero.
        /// </summary>
        public static Error BpmMustBeGreaterThanZero => Error.Validation(description: nameof(BpmMustBeGreaterThanZero));

        /// <summary>
        /// Gets the error that reports that the album cannot be added to the music artist because the artist already has it.
        /// </summary>
        public static Error TheArtistAlreadyHasTheAlbum => Error.Forbidden(description: nameof(TheArtistAlreadyHasTheAlbum));

        /// <summary>
        /// Gets the error that reports that the album cannot be removed from the music artist because the artist does not have it.
        /// </summary>
        public static Error TheArtistDoesNotHaveTheAlbum => Error.Forbidden(description: nameof(TheArtistDoesNotHaveTheAlbum));

        /// <summary>
        /// Gets the error that reports that the track cannot be added to the music album because the album already contains it.
        /// </summary>
        public static Error TheTrackIsAlreadyInTheAlbum => Error.Forbidden(description: nameof(TheTrackIsAlreadyInTheAlbum));

        /// <summary>
        /// Gets the error that reports that the track cannot be removed from the music album because the album does not contain it.
        /// </summary>
        public static Error TheTrackIsNotInTheAlbum => Error.Forbidden(description: nameof(TheTrackIsNotInTheAlbum));

        /// <summary>
        /// Gets the error that reports that the name of a music area cannot be empty.
        /// </summary>
        public static Error AreaNameCannotBeEmpty => Error.Validation(description: nameof(AreaNameCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that the name of a music artist alias cannot be empty.
        /// </summary>
        public static Error ArtistAliasNameCannotBeEmpty => Error.Validation(description: nameof(ArtistAliasNameCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that the name of a music artist alias exceeds the maximum length of 255 characters.
        /// </summary>
        public static Error ArtistAliasNameMustBeMaximum255CharactersLong => Error.Validation(description: nameof(ArtistAliasNameMustBeMaximum255CharactersLong));

        /// <summary>
        /// Gets the error that reports that the name of a music area exceeds the maximum length of 255 characters.
        /// </summary>
        public static Error AreaNameMustBeMaximum255CharactersLong => Error.Validation(description: nameof(AreaNameMustBeMaximum255CharactersLong));

        /// <summary>
        /// Gets the error that reports that a music artist type is not a recognized value.
        /// </summary>
        public static Error UnknownMusicArtistType => Error.Validation(description: nameof(UnknownMusicArtistType));

        /// <summary>
        /// Gets the error that reports that a music artist gender is not a recognized value.
        /// </summary>
        public static Error UnknownMusicArtistGender => Error.Validation(description: nameof(UnknownMusicArtistGender));

        /// <summary>
        /// Gets the error that reports that a music artist country exceeds the maximum length of 2 characters.
        /// </summary>
        public static Error CountryMustBeMaximum2CharactersLong => Error.Validation(description: nameof(CountryMustBeMaximum2CharactersLong));

        /// <summary>
        /// Gets the error that reports that an IPI value cannot be empty.
        /// </summary>
        public static Error IpiValueCannotBeEmpty => Error.Validation(description: nameof(IpiValueCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that an IPI value exceeds the maximum length of 20 characters.
        /// </summary>
        public static Error IpiValueMustBeMaximum20CharactersLong => Error.Validation(description: nameof(IpiValueMustBeMaximum20CharactersLong));

        /// <summary>
        /// Gets the error that reports that an ISNI value cannot be empty.
        /// </summary>
        public static Error IsniValueCannotBeEmpty => Error.Validation(description: nameof(IsniValueCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that an ISNI value exceeds the maximum length of 20 characters.
        /// </summary>
        public static Error IsniValueMustBeMaximum20CharactersLong => Error.Validation(description: nameof(IsniValueMustBeMaximum20CharactersLong));

        /// <summary>
        /// Gets the error that reports that the title of a music work cannot be empty.
        /// </summary>
        public static Error WorkTitleCannotBeEmpty => Error.Validation(description: nameof(WorkTitleCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that the cover image of a music artist or album cannot be <see langword="null"/>.
        /// </summary>
        public static Error CoverCannotBeNull => Error.Validation(description: nameof(CoverCannotBeNull));

        /// <summary>
        /// Gets the error that reports that the number of tracks requested for shuffling must be greater than zero.
        /// </summary>
        public static Error ShuffleTrackCountMustBeGreaterThanZero => Error.Validation(description: nameof(ShuffleTrackCountMustBeGreaterThanZero));

        /// <summary>
        /// Gets the error that reports that the number of tracks requested for shuffling cannot exceed 500.
        /// </summary>
        public static Error ShuffleTrackCountMustBeAtMost500 => Error.Validation(description: nameof(ShuffleTrackCountMustBeAtMost500));
    }
}
