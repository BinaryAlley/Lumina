#region ========================================================================= USING =====================================================================================
using Nemera.Domain.Common.Primitives;
#endregion

namespace Nemera.Domain.Common.Errors;

/// <summary>
/// Metadata error types.
/// </summary>
public static partial class Errors
{
    /// <summary>
    /// Defines the errors that report failures raised while working with the metadata of media items.
    /// </summary>
    public static class Metadata
    {
        /// <summary>
        /// Gets the error that reports that the metadata of a media item cannot be <see langword="null"/>.
        /// </summary>
        public static Error MetadataCannotBeNull => Error.Validation(description: nameof(MetadataCannotBeNull));

        /// <summary>
        /// Gets the error that reports that the title of a media item cannot be empty.
        /// </summary>
        public static Error TitleCannotBeEmpty => Error.Validation(description: nameof(TitleCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that the title of a media item exceeds the maximum length of 255 characters.
        /// </summary>
        public static Error TitleMustBeMaximum255CharactersLong => Error.Validation(description: nameof(TitleMustBeMaximum255CharactersLong));

        /// <summary>
        /// Gets the error that reports that the original title of a media item exceeds the maximum length of 255 characters.
        /// </summary>
        public static Error OriginalTitleMustBeMaximum255CharactersLong => Error.Validation(description: nameof(OriginalTitleMustBeMaximum255CharactersLong));

        /// <summary>
        /// Gets the error that reports that the description of a media item exceeds the maximum length of 2000 characters.
        /// </summary>
        public static Error DescriptionMustBeMaximum2000CharactersLong => Error.Validation(description: nameof(DescriptionMustBeMaximum2000CharactersLong));

        /// <summary>
        /// Gets the error that reports that the collection of genres for a media item cannot be <see langword="null"/>.
        /// </summary>
        public static Error GenresListCannotBeNull => Error.Validation(description: nameof(GenresListCannotBeNull));

        /// <summary>
        /// Gets the error that reports that the name of a genre cannot be empty.
        /// </summary>
        public static Error GenreNameCannotBeEmpty => Error.Validation(description: nameof(GenreNameCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that the name of a genre exceeds the maximum length of 50 characters.
        /// </summary>
        public static Error GenreNameMustBeMaximum50CharactersLong => Error.Validation(description: nameof(GenreNameMustBeMaximum50CharactersLong));

        /// <summary>
        /// Gets the error that reports that the collection of tags for a media item cannot be <see langword="null"/>.
        /// </summary>
        public static Error TagsListCannotBeNull => Error.Validation(description: nameof(TagsListCannotBeNull));

        /// <summary>
        /// Gets the error that reports that the name of a tag cannot be empty.
        /// </summary>
        public static Error TagNameCannotBeEmpty => Error.Validation(description: nameof(TagNameCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that the name of a tag exceeds the maximum length of 50 characters.
        /// </summary>
        public static Error TagNameMustBeMaximum50CharactersLong => Error.Validation(description: nameof(TagNameMustBeMaximum50CharactersLong));

        /// <summary>
        /// Gets the error that reports that the name of a mood cannot be empty.
        /// </summary>
        public static Error MoodNameCannotBeEmpty => Error.Validation(description: nameof(MoodNameCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that the code of a language cannot be empty.
        /// </summary>
        public static Error LanguageCodeCannotBeEmpty => Error.Validation(description: nameof(LanguageCodeCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that the name of a language cannot be empty.
        /// </summary>
        public static Error LanguageNameCannotBeEmpty => Error.Validation(description: nameof(LanguageNameCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that the code of a language must be exactly 2 characters long.
        /// </summary>
        public static Error LanguageCodeMustBe2CharactersLong => Error.Validation(description: nameof(LanguageCodeMustBe2CharactersLong));

        /// <summary>
        /// Gets the error that reports that the name of a language exceeds the maximum length of 50 characters.
        /// </summary>
        public static Error LanguageNameMustBeMaximum50CharactersLong => Error.Validation(description: nameof(LanguageNameMustBeMaximum50CharactersLong));

        /// <summary>
        /// Gets the error that reports that the native name of a language exceeds the maximum length of 50 characters.
        /// </summary>
        public static Error LanguageNativeNameMustBeMaximum50CharactersLong => Error.Validation(description: nameof(LanguageNativeNameMustBeMaximum50CharactersLong));

        /// <summary>
        /// Gets the error that reports that the original release year of a media item is outside the range from 1 to 9999.
        /// </summary>
        public static Error OriginalReleaseYearMustBeBetween1And9999 => Error.Validation(description: nameof(OriginalReleaseYearMustBeBetween1And9999));

        /// <summary>
        /// Gets the error that reports that the re-release year of a media item is outside the range from 1 to 9999.
        /// </summary>
        public static Error ReReleaseYearMustBeBetween1And9999 => Error.Validation(description: nameof(ReReleaseYearMustBeBetween1And9999));

        /// <summary>
        /// Gets the error that reports that the re-release year of a media item is earlier than its original release year.
        /// </summary>
        public static Error ReReleaseYearCannotBeEarlierThanOriginalReleaseYear => Error.Validation(description: nameof(ReReleaseYearCannotBeEarlierThanOriginalReleaseYear));

        /// <summary>
        /// Gets the error that reports that the code of a release country must be exactly 2 characters long.
        /// </summary>
        public static Error CountryCodeMustBe2CharactersLong => Error.Validation(description: nameof(CountryCodeMustBe2CharactersLong));

        /// <summary>
        /// Gets the error that reports that the release version of a media item exceeds the maximum length of 50 characters.
        /// </summary>
        public static Error ReleaseVersionMustBeMaximum50CharactersLong => Error.Validation(description: nameof(ReleaseVersionMustBeMaximum50CharactersLong));

        /// <summary>
        /// Gets the error that reports that the original release date and the original release year of a media item refer to different years.
        /// </summary>
        public static Error OriginalReleaseDateAndYearMustMatch => Error.Validation(description: nameof(OriginalReleaseDateAndYearMustMatch));

        /// <summary>
        /// Gets the error that reports that the re-release date and the re-release year of a media item refer to different years.
        /// </summary>
        public static Error ReReleaseDateAndYearMustMatch => Error.Validation(description: nameof(ReReleaseDateAndYearMustMatch));

        /// <summary>
        /// Gets the error that reports that the re-release date of a media item is earlier than its original release date.
        /// </summary>
        public static Error ReReleaseDateCannotBeEarlierThanOriginalReleaseDate => Error.Validation(description: nameof(ReReleaseDateCannotBeEarlierThanOriginalReleaseDate));

        /// <summary>
        /// Gets the error that reports that the release information of a media item cannot be <see langword="null"/>.
        /// </summary>
        public static Error ReleaseInfoCannotBeNull => Error.Validation(description: nameof(ReleaseInfoCannotBeNull));

        /// <summary>
        /// Gets the error that reports that the value or the maximum value of a rating is negative.
        /// </summary>
        public static Error RatingValueMustBePositive => Error.Validation(description: nameof(RatingValueMustBePositive));

        /// <summary>
        /// Gets the error that reports that the maximum value of a rating must be positive.
        /// </summary>
        public static Error RatingMaxValueMustBePositive => Error.Validation(description: nameof(RatingMaxValueMustBePositive));

        /// <summary>
        /// Gets the error that reports that the value of a rating is greater than the maximum value of the same rating.
        /// </summary>
        public static Error RatingValueCannotBeGreaterThanMaxValue => Error.Validation(description: nameof(RatingValueCannotBeGreaterThanMaxValue));

        /// <summary>
        /// Gets the error that reports that the vote count of a rating must be positive.
        /// </summary>
        public static Error RatingVoteCountMustBePositive => Error.Validation(description: nameof(RatingVoteCountMustBePositive));

        /// <summary>
        /// Gets the error that reports that the collection of ratings for a media item cannot be <see langword="null"/>.
        /// </summary>
        public static Error RatingsListCannotBeNull => Error.Validation(description: nameof(RatingsListCannotBeNull));
    }
}
