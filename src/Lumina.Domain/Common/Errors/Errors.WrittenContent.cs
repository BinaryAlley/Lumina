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
        public static Error BookAlreadyExists => Error.Conflict(description: nameof(BookAlreadyExists));
        public static Error IsbnValueCannotBeEmpty => Error.Validation(description: nameof(IsbnValueCannotBeEmpty));
        public static Error IsbnListCannotBeNull => Error.Validation(description: nameof(IsbnListCannotBeNull));
        public static Error InvalidIsbn10Format => Error.Validation(description: nameof(InvalidIsbn10Format));
        public static Error InvalidIsbn13Format => Error.Validation(description: nameof(InvalidIsbn13Format));
        public static Error UnknownIsbnFormat => Error.Unexpected(description: nameof(UnknownIsbnFormat));
        public static Error TheBookIsAlreadyInTheSeries => Error.Forbidden(description: nameof(TheBookIsAlreadyInTheSeries));
        public static Error TheBookIsNotInTheSeries => Error.Forbidden(description: nameof(TheBookIsNotInTheSeries));
        public static Error AsinMustBe10CharactersLong => Error.Validation(description: nameof(AsinMustBe10CharactersLong));
        public static Error GoodreadsIdMustBeNumeric => Error.Validation(description: nameof(GoodreadsIdMustBeNumeric));
        public static Error InvalidLccnFormat => Error.Validation(description: nameof(InvalidLccnFormat));
        public static Error InvalidOclcFormat => Error.Validation(description: nameof(InvalidOclcFormat));
        public static Error InvalidOpenLibraryId => Error.Validation(description: nameof(InvalidOpenLibraryId));
        public static Error LibraryThingIdMustBeMaximum50CharactersLong => Error.Validation(description: nameof(LibraryThingIdMustBeMaximum50CharactersLong));
        public static Error GoogleBooksIdMustBe12CharactersLong => Error.Validation(description: nameof(GoogleBooksIdMustBe12CharactersLong));
        public static Error InvalidGoogleBooksIdFormat => Error.Validation(description: nameof(InvalidGoogleBooksIdFormat));
        public static Error BarnesAndNoblesIdMustBe10CharactersLong => Error.Validation(description: nameof(BarnesAndNoblesIdMustBe10CharactersLong));
        public static Error InvalidBarnesAndNoblesIdFormat => Error.Validation(description: nameof(InvalidBarnesAndNoblesIdFormat));
        public static Error InvalidAppleBooksIdFormat => Error.Validation(description: nameof(InvalidAppleBooksIdFormat));
        public static Error PublisherMustBeMaximum100CharactersLong => Error.Validation(description: nameof(PublisherMustBeMaximum100CharactersLong));
        public static Error PageCountMustBeGreaterThanZero => Error.Validation(description: nameof(PageCountMustBeGreaterThanZero));
        public static Error UnknownBookFormat => Error.Validation(description: nameof(UnknownBookFormat));
        public static Error EditionMustBeMaximum50CharactersLong => Error.Validation(description: nameof(EditionMustBeMaximum50CharactersLong));
        public static Error VolumeNumberMustBeGreaterThanZero => Error.Validation(description: nameof(VolumeNumberMustBeGreaterThanZero));
    }
}
