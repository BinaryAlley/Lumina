#region ========================================================================= USING =====================================================================================
using ErrorOr;
#endregion

namespace Lumina.Domain.Common.Errors;

/// <summary>
/// Written content error types.
/// </summary>
public static partial class Errors
{
    public static class WrittenContent
    {
        #region ==================================================================== PROPERTIES =================================================================================
        public static Error BookAlreadyExists => Error.Conflict(nameof(BookAlreadyExists));
        public static Error IsbnValueCannotBeEmpty => Error.Validation(nameof(IsbnValueCannotBeEmpty));
        public static Error IsbnListCannotBeNull => Error.Validation(nameof(IsbnListCannotBeNull));
        public static Error InvalidIsbn10Format => Error.Validation(nameof(InvalidIsbn10Format));
        public static Error InvalidIsbn13Format => Error.Validation(nameof(InvalidIsbn13Format));
        public static Error UnknownIsbnFormat => Error.Unexpected(nameof(UnknownIsbnFormat));
        public static Error TheBookIsAlreadyInTheSeries => Error.Forbidden(nameof(TheBookIsAlreadyInTheSeries));
        public static Error TheBookIsNotInTheSeries => Error.Forbidden(nameof(TheBookIsNotInTheSeries));
        public static Error AsinMustBe10CharactersLong => Error.Validation(nameof(AsinMustBe10CharactersLong));
        public static Error GoodreadsIdMustBeNumeric => Error.Validation(nameof(GoodreadsIdMustBeNumeric));
        public static Error InvalidLccnFormat => Error.Validation(nameof(InvalidLccnFormat));
        public static Error InvalidOclcFormat => Error.Validation(nameof(InvalidOclcFormat));
        public static Error InvalidOpenLibraryId => Error.Validation(nameof(InvalidOpenLibraryId));
        public static Error LibraryThingIdMustBeMaximum50CharactersLong => Error.Validation(nameof(LibraryThingIdMustBeMaximum50CharactersLong));
        public static Error GoogleBooksIdMustBe12CharactersLong => Error.Validation(nameof(GoogleBooksIdMustBe12CharactersLong));
        public static Error InvalidGoogleBooksIdFormat => Error.Validation(nameof(InvalidGoogleBooksIdFormat));
        public static Error BarnesAndNoblesIdMustBe10CharactersLong => Error.Validation(nameof(BarnesAndNoblesIdMustBe10CharactersLong));
        public static Error InvalidBarnesAndNoblesIdFormat => Error.Validation(nameof(InvalidBarnesAndNoblesIdFormat));
        public static Error InvalidAppleBooksIdFormat => Error.Validation(nameof(InvalidAppleBooksIdFormat));
        public static Error PublisherMustBeMaximum100CharactersLong => Error.Validation(nameof(PublisherMustBeMaximum100CharactersLong));
        public static Error PageCountMustBeGreaterThanZero => Error.Validation(nameof(PageCountMustBeGreaterThanZero));
        public static Error UnknownBookFormat => Error.Validation(nameof(UnknownBookFormat));
        public static Error EditionMustBeMaximum50CharactersLong => Error.Validation(nameof(EditionMustBeMaximum50CharactersLong));
        public static Error VolumeNumberMustBeGreaterThanZero => Error.Validation(nameof(VolumeNumberMustBeGreaterThanZero));
        #endregion
    }
}