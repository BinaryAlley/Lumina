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
        public static Error ArtistNameCannotBeEmpty => Error.Validation(description: nameof(ArtistNameCannotBeEmpty));
        public static Error MusicBrainzIdInvalidFormat => Error.Validation(description: nameof(MusicBrainzIdInvalidFormat));
        public static Error IsrcValueCannotBeEmpty => Error.Validation(description: nameof(IsrcValueCannotBeEmpty));
        public static Error IsrcInvalidFormat => Error.Validation(description: nameof(IsrcInvalidFormat));
        public static Error BarcodeValueCannotBeEmpty => Error.Validation(description: nameof(BarcodeValueCannotBeEmpty));
        public static Error InvalidFormatForBarcode => Error.Validation(description: nameof(InvalidFormatForBarcode));
        public static Error CreditRoleDisplayNameCannotBeEmpty => Error.Validation(description: nameof(CreditRoleDisplayNameCannotBeEmpty));
        public static Error TheArtistAlreadyHasTheAlbum => Error.Forbidden(description: nameof(TheArtistAlreadyHasTheAlbum));
        public static Error TheArtistDoesNotHaveTheAlbum => Error.Forbidden(description: nameof(TheArtistDoesNotHaveTheAlbum));
        public static Error TheTrackIsAlreadyInTheAlbum => Error.Forbidden(description: nameof(TheTrackIsAlreadyInTheAlbum));
        public static Error TheTrackIsNotInTheAlbum => Error.Forbidden(description: nameof(TheTrackIsNotInTheAlbum));
    }
}
