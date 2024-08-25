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
        #region ==================================================================== PROPERTIES =================================================================================
        public static Error MetadataCannotBeNull => Error.Validation(nameof(MetadataCannotBeNull));
        public static Error TitleCannotBeEmpty => Error.Validation(nameof(TitleCannotBeEmpty));
        public static Error TitleMustBeMaximum255CharactersLong => Error.Validation(nameof(TitleMustBeMaximum255CharactersLong));
        public static Error OriginalTitleMustBeMaximum255CharactersLong => Error.Validation(nameof(OriginalTitleMustBeMaximum255CharactersLong));
        public static Error DescriptionMustBeMaximum2000CharactersLong => Error.Validation(nameof(DescriptionMustBeMaximum2000CharactersLong));
        public static Error GenresListCannotBeNull => Error.Validation(nameof(GenresListCannotBeNull));
        public static Error GenreNameCannotBeEmpty => Error.Validation(nameof(GenreNameCannotBeEmpty));
        public static Error GenreNameMustBeMaximum50CharactersLong => Error.Validation(nameof(GenreNameMustBeMaximum50CharactersLong));
        public static Error TagsListCannotBeNull => Error.Validation(nameof(TagsListCannotBeNull));
        public static Error TagNameCannotBeEmpty => Error.Validation(nameof(TagNameCannotBeEmpty));
        public static Error TagNameMustBeMaximum50CharactersLong => Error.Validation(nameof(TagNameMustBeMaximum50CharactersLong));
        public static Error LanguageCodeCannotBeEmpty => Error.Validation(nameof(LanguageCodeCannotBeEmpty));
        public static Error LanguageNameCannotBeEmpty => Error.Validation(nameof(LanguageNameCannotBeEmpty));
        public static Error LanguageCodeMustBe2CharactersLong => Error.Validation(nameof(LanguageCodeMustBe2CharactersLong));
        public static Error LanguageNameMustBeMaximum50CharactersLong => Error.Validation(nameof(LanguageNameMustBeMaximum50CharactersLong));
        public static Error LanguageNativeNameMustBeMaximum50CharactersLong => Error.Validation(nameof(LanguageNativeNameMustBeMaximum50CharactersLong));
        public static Error OriginalReleaseYearMustBeBetween1And9999 => Error.Validation(nameof(OriginalReleaseYearMustBeBetween1And9999));
        public static Error ReReleaseYearMustBeBetween1And9999 => Error.Validation(nameof(ReReleaseYearMustBeBetween1And9999));
        public static Error ReReleaseYearCannotBeEarlierThanOriginalReleaseYear => Error.Validation(nameof(ReReleaseYearCannotBeEarlierThanOriginalReleaseYear));
        public static Error CountryCodeMustBe2CharactersLong => Error.Validation(nameof(CountryCodeMustBe2CharactersLong));
        public static Error ReleaseVersionMustBeMaximum50CharactersLong => Error.Validation(nameof(ReleaseVersionMustBeMaximum50CharactersLong));
        public static Error OriginalReleaseDateAndYearMustMatch => Error.Validation(nameof(OriginalReleaseDateAndYearMustMatch));
        public static Error ReReleaseDateAndYearMustMatch => Error.Validation(nameof(ReReleaseDateAndYearMustMatch));
        public static Error ReReleaseDateCannotBeEarlierThanOriginalReleaseDate => Error.Validation(nameof(ReReleaseDateCannotBeEarlierThanOriginalReleaseDate));
        public static Error ReleaseInfoCannotBeNull => Error.Validation(nameof(ReleaseInfoCannotBeNull));
        public static Error RatingValueMustBePositive => Error.Validation(nameof(RatingValueMustBePositive));
        public static Error RatingMaxValueMustBePositive => Error.Validation(nameof(RatingMaxValueMustBePositive));
        public static Error RatingValueCannotBeGreaterThanMaxValue => Error.Validation(nameof(RatingValueCannotBeGreaterThanMaxValue));
        public static Error RatingVoteCountMustBePositive => Error.Validation(nameof(RatingVoteCountMustBePositive));
        public static Error RatingsListCannotBeNull => Error.Validation(nameof(RatingsListCannotBeNull));
        public static Error InvalidIsoCode => Error.Validation(nameof(InvalidIsoCode));
        #endregion
    }
}