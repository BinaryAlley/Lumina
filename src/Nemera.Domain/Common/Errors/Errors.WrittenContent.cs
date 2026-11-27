#region ========================================================================= USING =====================================================================================
using Nemera.Domain.Common.Primitives;
#endregion

namespace Nemera.Domain.Common.Errors;

/// <summary>
/// Written content error types.
/// </summary>
public static partial class Errors
{
    /// <summary>
    /// Defines the errors that report failures raised while working with books and book series.
    /// </summary>
    public static class WrittenContent
    {
        /// <summary>
        /// Gets the error that reports that a book with the same identifier or file path already exists in the library.
        /// </summary>
        public static Error BookAlreadyExists => Error.Conflict(description: nameof(BookAlreadyExists));

        /// <summary>
        /// Gets the error that reports that an ISBN value cannot be empty.
        /// </summary>
        public static Error IsbnValueCannotBeEmpty => Error.Validation(description: nameof(IsbnValueCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that the collection of ISBNs for a book cannot be <see langword="null"/>.
        /// </summary>
        public static Error IsbnListCannotBeNull => Error.Validation(description: nameof(IsbnListCannotBeNull));

        /// <summary>
        /// Gets the error that reports that an ISBN-10 value does not have a valid ISBN-10 format.
        /// </summary>
        public static Error InvalidIsbn10Format => Error.Validation(description: nameof(InvalidIsbn10Format));

        /// <summary>
        /// Gets the error that reports that an ISBN-13 value does not have a valid ISBN-13 format.
        /// </summary>
        public static Error InvalidIsbn13Format => Error.Validation(description: nameof(InvalidIsbn13Format));

        /// <summary>
        /// Gets the error that reports that an ISBN format is not a recognized ISBN-10 or ISBN-13 value.
        /// </summary>
        public static Error UnknownIsbnFormat => Error.Unexpected(description: nameof(UnknownIsbnFormat));

        /// <summary>
        /// Gets the error that reports that the book cannot be added to the series because the series already contains it.
        /// </summary>
        public static Error TheBookIsAlreadyInTheSeries => Error.Forbidden(description: nameof(TheBookIsAlreadyInTheSeries));

        /// <summary>
        /// Gets the error that reports that the book cannot be removed from the series because the series does not contain it.
        /// </summary>
        public static Error TheBookIsNotInTheSeries => Error.Forbidden(description: nameof(TheBookIsNotInTheSeries));

        /// <summary>
        /// Gets the error that reports that an ASIN must be exactly 10 characters long.
        /// </summary>
        public static Error AsinMustBe10CharactersLong => Error.Validation(description: nameof(AsinMustBe10CharactersLong));

        /// <summary>
        /// Gets the error that reports that a Goodreads identifier is not made of digits.
        /// </summary>
        public static Error GoodreadsIdMustBeNumeric => Error.Validation(description: nameof(GoodreadsIdMustBeNumeric));

        /// <summary>
        /// Gets the error that reports that an LCCN does not have a valid Library of Congress Control Number format.
        /// </summary>
        public static Error InvalidLccnFormat => Error.Validation(description: nameof(InvalidLccnFormat));

        /// <summary>
        /// Gets the error that reports that an OCLC number does not have a valid OCLC format.
        /// </summary>
        public static Error InvalidOclcFormat => Error.Validation(description: nameof(InvalidOclcFormat));

        /// <summary>
        /// Gets the error that reports that an Open Library identifier does not have a valid format.
        /// </summary>
        public static Error InvalidOpenLibraryId => Error.Validation(description: nameof(InvalidOpenLibraryId));

        /// <summary>
        /// Gets the error that reports that a LibraryThing identifier exceeds the maximum length of 50 characters.
        /// </summary>
        public static Error LibraryThingIdMustBeMaximum50CharactersLong => Error.Validation(description: nameof(LibraryThingIdMustBeMaximum50CharactersLong));

        /// <summary>
        /// Gets the error that reports that a Google Books identifier must be exactly 12 characters long.
        /// </summary>
        public static Error GoogleBooksIdMustBe12CharactersLong => Error.Validation(description: nameof(GoogleBooksIdMustBe12CharactersLong));

        /// <summary>
        /// Gets the error that reports that a Google Books identifier contains characters other than letters, digits, underscores and hyphens.
        /// </summary>
        public static Error InvalidGoogleBooksIdFormat => Error.Validation(description: nameof(InvalidGoogleBooksIdFormat));

        /// <summary>
        /// Gets the error that reports that a Barnes and Noble identifier must be exactly 10 characters long.
        /// </summary>
        public static Error BarnesAndNoblesIdMustBe10CharactersLong => Error.Validation(description: nameof(BarnesAndNoblesIdMustBe10CharactersLong));

        /// <summary>
        /// Gets the error that reports that a Barnes and Noble identifier is not made of digits.
        /// </summary>
        public static Error InvalidBarnesAndNoblesIdFormat => Error.Validation(description: nameof(InvalidBarnesAndNoblesIdFormat));

        /// <summary>
        /// Gets the error that reports that an Apple Books identifier does not have the expected id prefix followed by digits.
        /// </summary>
        public static Error InvalidAppleBooksIdFormat => Error.Validation(description: nameof(InvalidAppleBooksIdFormat));

        /// <summary>
        /// Gets the error that reports that the publisher of a book exceeds the maximum length of 100 characters.
        /// </summary>
        public static Error PublisherMustBeMaximum100CharactersLong => Error.Validation(description: nameof(PublisherMustBeMaximum100CharactersLong));

        /// <summary>
        /// Gets the error that reports that the page count of a book must be greater than zero.
        /// </summary>
        public static Error PageCountMustBeGreaterThanZero => Error.Validation(description: nameof(PageCountMustBeGreaterThanZero));

        /// <summary>
        /// Gets the error that reports that a book format is not a recognized value.
        /// </summary>
        public static Error UnknownBookFormat => Error.Validation(description: nameof(UnknownBookFormat));

        /// <summary>
        /// Gets the error that reports that the edition of a book exceeds the maximum length of 50 characters.
        /// </summary>
        public static Error EditionMustBeMaximum50CharactersLong => Error.Validation(description: nameof(EditionMustBeMaximum50CharactersLong));

        /// <summary>
        /// Gets the error that reports that the volume number of a book must be greater than zero.
        /// </summary>
        public static Error VolumeNumberMustBeGreaterThanZero => Error.Validation(description: nameof(VolumeNumberMustBeGreaterThanZero));

        /// <summary>
        /// Gets the error that reports that the book library cannot be <see langword="null"/>.
        /// </summary>
        public static Error BookLibraryCannotBeNull => Error.Validation(description: nameof(BookLibraryCannotBeNull));

        /// <summary>
        /// Gets the error that reports that the file path of a book cannot be empty.
        /// </summary>
        public static Error BookPathCannotBeEmpty => Error.Validation(description: nameof(BookPathCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that the file path of a book exceeds the maximum length of 2048 characters.
        /// </summary>
        public static Error BookPathMustBeMaximum2048CharactersLong => Error.Validation(description: nameof(BookPathMustBeMaximum2048CharactersLong));

        /// <summary>
        /// Gets the error that reports that a book file path does not reside within any of the content locations of the library.
        /// </summary>
        public static Error BookPathMustBeWithinLibraryContentLocations => Error.Validation(description: nameof(BookPathMustBeWithinLibraryContentLocations));

        /// <summary>
        /// Gets the error that reports that a book identifier cannot be empty.
        /// </summary>
        public static Error BookIdCannotBeEmpty => Error.Validation(description: nameof(BookIdCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that the cover image of a book cannot be <see langword="null"/>.
        /// </summary>
        public static Error BookCoverCannotBeNull => Error.Validation(description: nameof(BookCoverCannotBeNull));

        /// <summary>
        /// Gets the error that reports that the requested book does not exist.
        /// </summary>
        public static Error BookNotFound => Error.NotFound(description: nameof(BookNotFound));
    }
}
