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
        public static Error TagCannotBeEmpty => Error.Validation(nameof(TagCannotBeEmpty));
        public static Error GenreCannotBeEmpty => Error.Validation(nameof(GenreCannotBeEmpty));
        public static Error OriginalReleaseDateAndYearMustMatch => Error.Validation(nameof(OriginalReleaseDateAndYearMustMatch));
        public static Error ReReleaseDateAndYearMustMatch => Error.Validation(nameof(ReReleaseDateAndYearMustMatch));
        public static Error ReReleaseDateCannotBeEarlierThanOriginalReleaseDate => Error.Validation(nameof(ReReleaseDateCannotBeEarlierThanOriginalReleaseDate));
        public static Error ReReleaseYearCannotBeEarlierThanOriginalReleaseYear => Error.Validation(nameof(ReReleaseYearCannotBeEarlierThanOriginalReleaseYear));
        public static Error RatingMustBePositive => Error.Validation(nameof(RatingMustBePositive));
        public static Error RatingValueCannotBeGreaterThanMaxValue => Error.Validation(nameof(RatingValueCannotBeGreaterThanMaxValue));
        public static Error LanguageCodeCannotBeEmpty => Error.Validation(nameof(LanguageCodeCannotBeEmpty));
        public static Error LanguageNameCannotBeEmpty => Error.Validation(nameof(LanguageNameCannotBeEmpty));
        public static Error InvalidIsoCode => Error.Validation(nameof(InvalidIsoCode));
        #endregion
    }
}