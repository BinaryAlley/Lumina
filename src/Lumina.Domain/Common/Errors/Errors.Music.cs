#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
#endregion

namespace Lumina.Domain.Common.Errors;

/// <summary>
/// Music error types.
/// </summary>
public static partial class Errors
{
    public static class Music
    {
        public static Error ArtistNotFound => Error.NotFound(description: nameof(ArtistNotFound));
        public static Error ArtistAlreadyExists => Error.Conflict(description: nameof(ArtistAlreadyExists));
        public static Error ArtistIdCannotBeEmpty => Error.Validation(description: nameof(ArtistIdCannotBeEmpty));
        public static Error ArtistNameCannotBeEmpty => Error.Validation(description: nameof(ArtistNameCannotBeEmpty));
        public static Error ArtistNameMustBeMaximum255CharactersLong => Error.Validation(description: nameof(ArtistNameMustBeMaximum255CharactersLong));
        public static Error ArtistWebsiteMustBeMaximum2048CharactersLong => Error.Validation(description: nameof(ArtistWebsiteMustBeMaximum2048CharactersLong));
        public static Error MusicBrainzIdInvalidFormat => Error.Validation(description: nameof(MusicBrainzIdInvalidFormat));
        public static Error IsrcValueCannotBeEmpty => Error.Validation(description: nameof(IsrcValueCannotBeEmpty));
        public static Error IsrcInvalidFormat => Error.Validation(description: nameof(IsrcInvalidFormat));
        public static Error BarcodeValueCannotBeEmpty => Error.Validation(description: nameof(BarcodeValueCannotBeEmpty));
        public static Error InvalidFormatForBarcode => Error.Validation(description: nameof(InvalidFormatForBarcode));
        public static Error AlbumNotFound => Error.NotFound(description: nameof(AlbumNotFound));
        public static Error AlbumAlreadyExists => Error.Conflict(description: nameof(AlbumAlreadyExists));
        public static Error ArtistMustHaveAtLeastOneAlbum => Error.Forbidden(description: nameof(ArtistMustHaveAtLeastOneAlbum));
        public static Error AlbumIdCannotBeEmpty => Error.Validation(description: nameof(AlbumIdCannotBeEmpty));
        public static Error AlbumsListCannotBeNull => Error.Validation(description: nameof(AlbumsListCannotBeNull));
        public static Error AlbumTitleCannotBeEmpty => Error.Validation(description: nameof(AlbumTitleCannotBeEmpty));
        public static Error AlbumTitleMustBeMaximum255CharactersLong => Error.Validation(description: nameof(AlbumTitleMustBeMaximum255CharactersLong));
        public static Error TotalDiscsMustBeGreaterThanZero => Error.Validation(description: nameof(TotalDiscsMustBeGreaterThanZero));
        public static Error TotalTracksMustBeGreaterThanZero => Error.Validation(description: nameof(TotalTracksMustBeGreaterThanZero));
        public static Error UnknownMusicReleaseType => Error.Validation(description: nameof(UnknownMusicReleaseType));
        public static Error UnknownMusicReleaseStatus => Error.Validation(description: nameof(UnknownMusicReleaseStatus));
        public static Error UnknownMusicMediaFormat => Error.Validation(description: nameof(UnknownMusicMediaFormat));
        public static Error UnknownMusicKey => Error.Validation(description: nameof(UnknownMusicKey));
        public static Error CatalogNumberMustBeMaximum50CharactersLong => Error.Validation(description: nameof(CatalogNumberMustBeMaximum50CharactersLong));
        public static Error TrackNotFound => Error.NotFound(description: nameof(TrackNotFound));
        public static Error TrackAlreadyExists => Error.Conflict(description: nameof(TrackAlreadyExists));
        public static Error TrackIdCannotBeEmpty => Error.Validation(description: nameof(TrackIdCannotBeEmpty));
        public static Error TracksListCannotBeNull => Error.Validation(description: nameof(TracksListCannotBeNull));
        public static Error TrackPathCannotBeEmpty => Error.Validation(description: nameof(TrackPathCannotBeEmpty));
        public static Error TrackPathMustBeMaximum2048CharactersLong => Error.Validation(description: nameof(TrackPathMustBeMaximum2048CharactersLong));
        public static Error TrackPathMustBeWithinLibraryContentLocations => Error.Validation(description: nameof(TrackPathMustBeWithinLibraryContentLocations));
        public static Error TrackNumberMustBeGreaterThanZero => Error.Validation(description: nameof(TrackNumberMustBeGreaterThanZero));
        public static Error DiscNumberMustBeGreaterThanZero => Error.Validation(description: nameof(DiscNumberMustBeGreaterThanZero));
        public static Error ScriptMustBeMaximum50CharactersLong => Error.Validation(description: nameof(ScriptMustBeMaximum50CharactersLong));
        public static Error WorkMustBeMaximum255CharactersLong => Error.Validation(description: nameof(WorkMustBeMaximum255CharactersLong));
        public static Error BpmMustBeGreaterThanZero => Error.Validation(description: nameof(BpmMustBeGreaterThanZero));
        public static Error TheArtistAlreadyHasTheAlbum => Error.Forbidden(description: nameof(TheArtistAlreadyHasTheAlbum));
        public static Error TheArtistDoesNotHaveTheAlbum => Error.Forbidden(description: nameof(TheArtistDoesNotHaveTheAlbum));
        public static Error TheTrackIsAlreadyInTheAlbum => Error.Forbidden(description: nameof(TheTrackIsAlreadyInTheAlbum));
        public static Error TheTrackIsNotInTheAlbum => Error.Forbidden(description: nameof(TheTrackIsNotInTheAlbum));
    }
}
