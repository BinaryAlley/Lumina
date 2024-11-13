#region ========================================================================= USING =====================================================================================
using ErrorOr;
#endregion

namespace Lumina.Domain.Common.Errors;

/// <summary>
/// Metadata error types.
/// </summary>
public static partial class Errors
{
    public static class Metadata
    {
        public static Error MetadataCannotBeNull => Error.Validation(description: nameof(MetadataCannotBeNull));
        public static Error TitleCannotBeEmpty => Error.Validation(description: nameof(TitleCannotBeEmpty));
        public static Error TitleMustBeMaximum255CharactersLong => Error.Validation(description: nameof(TitleMustBeMaximum255CharactersLong));
        public static Error OriginalTitleMustBeMaximum255CharactersLong => Error.Validation(description: nameof(OriginalTitleMustBeMaximum255CharactersLong));
        public static Error DescriptionMustBeMaximum2000CharactersLong => Error.Validation(description: nameof(DescriptionMustBeMaximum2000CharactersLong));
        public static Error GenresListCannotBeNull => Error.Validation(description: nameof(GenresListCannotBeNull));
        public static Error GenreNameCannotBeEmpty => Error.Validation(description: nameof(GenreNameCannotBeEmpty));
        public static Error GenreNameMustBeMaximum50CharactersLong => Error.Validation(description: nameof(GenreNameMustBeMaximum50CharactersLong));
        public static Error TagsListCannotBeNull => Error.Validation(description: nameof(TagsListCannotBeNull));
        public static Error TagNameCannotBeEmpty => Error.Validation(description: nameof(TagNameCannotBeEmpty));
        public static Error TagNameMustBeMaximum50CharactersLong => Error.Validation(description: nameof(TagNameMustBeMaximum50CharactersLong));
        public static Error LanguageCodeCannotBeEmpty => Error.Validation(description: nameof(LanguageCodeCannotBeEmpty));
        public static Error LanguageNameCannotBeEmpty => Error.Validation(description: nameof(LanguageNameCannotBeEmpty));
        public static Error LanguageCodeMustBe2CharactersLong => Error.Validation(description: nameof(LanguageCodeMustBe2CharactersLong));
        public static Error LanguageNameMustBeMaximum50CharactersLong => Error.Validation(description: nameof(LanguageNameMustBeMaximum50CharactersLong));
        public static Error LanguageNativeNameMustBeMaximum50CharactersLong => Error.Validation(description: nameof(LanguageNativeNameMustBeMaximum50CharactersLong));
        public static Error OriginalReleaseYearMustBeBetween1And9999 => Error.Validation(description: nameof(OriginalReleaseYearMustBeBetween1And9999));
        public static Error ReReleaseYearMustBeBetween1And9999 => Error.Validation(description: nameof(ReReleaseYearMustBeBetween1And9999));
        public static Error ReReleaseYearCannotBeEarlierThanOriginalReleaseYear => Error.Validation(description: nameof(ReReleaseYearCannotBeEarlierThanOriginalReleaseYear));
        public static Error CountryCodeMustBe2CharactersLong => Error.Validation(description: nameof(CountryCodeMustBe2CharactersLong));
        public static Error ReleaseVersionMustBeMaximum50CharactersLong => Error.Validation(description: nameof(ReleaseVersionMustBeMaximum50CharactersLong));
        public static Error OriginalReleaseDateAndYearMustMatch => Error.Validation(description: nameof(OriginalReleaseDateAndYearMustMatch));
        public static Error ReReleaseDateAndYearMustMatch => Error.Validation(description: nameof(ReReleaseDateAndYearMustMatch));
        public static Error ReReleaseDateCannotBeEarlierThanOriginalReleaseDate => Error.Validation(description: nameof(ReReleaseDateCannotBeEarlierThanOriginalReleaseDate));
        public static Error ReleaseInfoCannotBeNull => Error.Validation(description: nameof(ReleaseInfoCannotBeNull));
        public static Error RatingValueMustBePositive => Error.Validation(description: nameof(RatingValueMustBePositive));
        public static Error RatingMaxValueMustBePositive => Error.Validation(description: nameof(RatingMaxValueMustBePositive));
        public static Error RatingValueCannotBeGreaterThanMaxValue => Error.Validation(description: nameof(RatingValueCannotBeGreaterThanMaxValue));
        public static Error RatingVoteCountMustBePositive => Error.Validation(description: nameof(RatingVoteCountMustBePositive));
        public static Error RatingsListCannotBeNull => Error.Validation(description: nameof(RatingsListCannotBeNull));
        public static Error InvalidIsoCode => Error.Validation(description: nameof(InvalidIsoCode));
    }
}
