#region ========================================================================= USING =====================================================================================
using FluentValidation;
using Lumina.Domain.Common.Enums.BookLibrary;
using Lumina.Domain.Common.Errors;
#endregion

namespace Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BooksLibrary.Books.Commands.AddBook;

/// <summary>
/// Validates the needed validation rules for <see cref="AddBookCommand"/>.
/// </summary>
public class AddBookCommandValidator : AbstractValidator<AddBookCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddBookCommandValidator"/> class.
    /// </summary>
    public AddBookCommandValidator()
    {
        RuleFor(x => x.Metadata)
            .NotNull().WithMessage(Errors.Metadata.MetadataCannotBeNull.Description)
            .ChildRules(metadata =>
            {
                metadata.RuleFor(m => m!.Title)
                    .NotEmpty().WithMessage(Errors.Metadata.TitleCannotBeEmpty.Description)
                    .MaximumLength(255).WithMessage(Errors.Metadata.TitleMustBeMaximum255CharactersLong.Description);
                metadata.RuleFor(m => m!.OriginalTitle)
                    .MaximumLength(255).When(m => m!.OriginalTitle is not null)
                    .WithMessage(Errors.Metadata.OriginalTitleMustBeMaximum255CharactersLong.Description);
                metadata.RuleFor(m => m!.Description)
                    .MaximumLength(2000).When(m => m!.Description is not null)
                    .WithMessage(Errors.Metadata.DescriptionMustBeMaximum2000CharactersLong.Description);
                metadata.RuleFor(m => m!.ReleaseInfo)
                    .NotNull().WithMessage(Errors.Metadata.ReleaseInfoCannotBeNull.Description)
                    .ChildRules(ri =>
                    {
                        ri.RuleFor(r => r!.OriginalReleaseYear)
                            .InclusiveBetween(1, 9999).When(r => r!.OriginalReleaseYear.HasValue)
                            .WithMessage(Errors.Metadata.OriginalReleaseYearMustBeBetween1And9999.Description);
                        ri.RuleFor(r => r!.ReReleaseYear)
                            .InclusiveBetween(1, 9999).When(r => r!.ReReleaseYear.HasValue)
                            .WithMessage(Errors.Metadata.ReReleaseYearMustBeBetween1And9999.Description);
                        ri.RuleFor(r => r!.ReleaseCountry)
                            .Matches("^[A-Za-z]{2}$").When(r => r!.ReleaseCountry is not null)
                            .WithMessage(Errors.Metadata.CountryCodeMustBe2CharactersLong.Description);
                        ri.RuleFor(r => r!.ReleaseVersion)
                            .MaximumLength(50).When(r => r!.ReleaseVersion is not null)
                            .WithMessage(Errors.Metadata.ReleaseVersionMustBeMaximum50CharactersLong.Description);
                        ri.RuleFor(r => r!.OriginalReleaseYear)
                            .Must((releaseInfo, originalReleaseYear) =>
                                !releaseInfo!.OriginalReleaseDate.HasValue ||
                                !releaseInfo.OriginalReleaseYear.HasValue ||
                                originalReleaseYear == releaseInfo.OriginalReleaseDate.Value.Year)
                            .When(r => r!.OriginalReleaseDate.HasValue && r.OriginalReleaseYear.HasValue)
                            .WithMessage(Errors.Metadata.OriginalReleaseDateAndYearMustMatch.Description);
                        ri.RuleFor(r => r!.ReReleaseYear)
                            .Must((releaseInfo, reReleaseYear) =>
                                !releaseInfo!.ReReleaseDate.HasValue ||
                                !releaseInfo.ReReleaseYear.HasValue ||
                                reReleaseYear == releaseInfo.ReReleaseDate.Value.Year)
                            .When(r => r!.ReReleaseDate.HasValue && r.ReReleaseYear.HasValue)
                            .WithMessage(Errors.Metadata.ReReleaseDateAndYearMustMatch.Description);
                        ri.RuleFor(r => r!.ReReleaseYear)
                          .Must((releaseInfo, reReleaseYear) =>
                              !releaseInfo!.ReReleaseYear.HasValue ||
                              !releaseInfo.OriginalReleaseYear.HasValue ||
                              reReleaseYear >= releaseInfo.OriginalReleaseYear)
                          .When(r => r!.ReReleaseYear.HasValue && r.OriginalReleaseYear.HasValue)
                          .WithMessage(Errors.Metadata.ReReleaseYearCannotBeEarlierThanOriginalReleaseYear.Description);
                        ri.RuleFor(r => r!.ReReleaseDate)
                            .Must((releaseInfo, reReleaseDate) =>
                                !releaseInfo!.ReReleaseDate.HasValue ||
                                !releaseInfo.OriginalReleaseDate.HasValue ||
                                reReleaseDate >= releaseInfo.OriginalReleaseDate)
                            .When(r => r!.ReReleaseDate.HasValue && r.OriginalReleaseDate.HasValue)
                            .WithMessage(Errors.Metadata.ReReleaseDateCannotBeEarlierThanOriginalReleaseDate.Description);
                        ri.RuleFor(r => r!.ReReleaseYear)
                            .Must((releaseInfo, reReleaseYear) =>
                                !releaseInfo!.ReReleaseDate.HasValue ||
                                !releaseInfo.ReReleaseYear.HasValue ||
                                reReleaseYear == releaseInfo.ReReleaseDate.Value.Year)
                            .When(r => r!.ReReleaseDate.HasValue && r.ReReleaseYear.HasValue)
                            .WithMessage(Errors.Metadata.ReReleaseDateAndYearMustMatch.Description);
                    });
                metadata.RuleFor(m => m!.Genres)
                    .NotNull().WithMessage(Errors.Metadata.GenresListCannotBeNull.Description);
                metadata.RuleForEach(m => m!.Genres)
                    .ChildRules(g =>
                        g.RuleFor(genre => genre.Name)
                            .NotEmpty().WithMessage(Errors.Metadata.GenreNameCannotBeEmpty.Description)
                            .MaximumLength(50).WithMessage(Errors.Metadata.GenreNameMustBeMaximum50CharactersLong.Description));
                metadata.RuleFor(m => m!.Tags)
                    .NotNull().WithMessage(Errors.Metadata.TagsListCannotBeNull.Description);
                metadata.RuleForEach(m => m!.Tags)
                    .ChildRules(t =>
                        t.RuleFor(tag => tag.Name)
                            .NotEmpty().WithMessage(Errors.Metadata.TagNameCannotBeEmpty.Description)
                            .MaximumLength(50).WithMessage(Errors.Metadata.TagNameMustBeMaximum50CharactersLong.Description));
                metadata.RuleFor(m => m!.Language)
                    .Custom((language, context) =>
                    {
                        if (language is null)
                            return;

                        if (string.IsNullOrEmpty(language.LanguageCode))
                            context.AddFailure("Language.LanguageCode", Errors.Metadata.LanguageCodeCannotBeEmpty.Description);
                        else if (language.LanguageCode.Length > 2)
                            context.AddFailure("Language.LanguageCode", Errors.Metadata.LanguageCodeMustBe2CharactersLong.Description);

                        if (string.IsNullOrEmpty(language.LanguageName))
                            context.AddFailure("Language.LanguageName", Errors.Metadata.LanguageNameCannotBeEmpty.Description);
                        else if (language.LanguageName.Length > 50)
                            context.AddFailure("Language.LanguageName", Errors.Metadata.LanguageNameMustBeMaximum50CharactersLong.Description);

                        if (language.NativeName != null && language.NativeName.Length > 50)
                            context.AddFailure("Language.NativeName", Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong.Description);
                    });
                metadata.RuleFor(m => m!.OriginalLanguage)
                    .Custom((language, context) =>
                    {
                        if (language is null)
                            return;

                        if (string.IsNullOrEmpty(language.LanguageCode))
                            context.AddFailure("OriginalLanguage.LanguageCode", Errors.Metadata.LanguageCodeCannotBeEmpty.Description);
                        else if (language.LanguageCode.Length > 2)
                            context.AddFailure("OriginalLanguage.LanguageCode", Errors.Metadata.LanguageCodeMustBe2CharactersLong.Description);

                        if (string.IsNullOrEmpty(language.LanguageName))
                            context.AddFailure("OriginalLanguage.LanguageName", Errors.Metadata.LanguageNameCannotBeEmpty.Description);
                        else if (language.LanguageName.Length > 50)
                            context.AddFailure("OriginalLanguage.LanguageName", Errors.Metadata.LanguageNameMustBeMaximum50CharactersLong.Description);

                        if (language.NativeName != null && language.NativeName.Length > 50)
                            context.AddFailure("OriginalLanguage.NativeName", Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong.Description);
                    });
                metadata.RuleFor(m => m!.Publisher)
                    .MaximumLength(100).When(m => m!.Publisher is not null)
                    .WithMessage(Errors.WrittenContent.PublisherMustBeMaximum100CharactersLong.Description);
                metadata.RuleFor(m => m!.PageCount)
                    .GreaterThan(0).When(m => m!.PageCount.HasValue)
                    .WithMessage(Errors.WrittenContent.PageCountMustBeGreaterThanZero.Description);
            });
        RuleFor(x => x.Format)
            .IsInEnum().When(f => f.Format is not null).WithMessage(Errors.WrittenContent.UnknownBookFormat.Description);
        RuleFor(x => x.Edition)
            .MaximumLength(50).When(x => x.Edition is not null)
            .WithMessage(Errors.WrittenContent.EditionMustBeMaximum50CharactersLong.Description);
        RuleFor(x => x.VolumeNumber)
            .GreaterThan(0).When(x => x.VolumeNumber.HasValue)
            .WithMessage(Errors.WrittenContent.VolumeNumberMustBeGreaterThanZero.Description);
        RuleFor(x => x.Series)
            .ChildRules(s =>
                s.RuleFor(series => series!.Title)
                    .NotEmpty().WithMessage(Errors.Metadata.TitleCannotBeEmpty.Description)
                    .MaximumLength(255)).WithMessage(Errors.Metadata.TitleMustBeMaximum255CharactersLong.Description)
            .When(x => x.Series is not null);
        RuleFor(x => x.ASIN)
            .Length(10).When(x => x.ASIN is not null)
            .WithMessage(Errors.WrittenContent.AsinMustBe10CharactersLong.Description);
        RuleFor(x => x.GoodreadsId)
            .Matches(@"^\d+$").When(x => x.GoodreadsId is not null)
            .WithMessage(Errors.WrittenContent.GoodreadsIdMustBeNumeric.Description);
        RuleFor(x => x.LCCN)
            .Matches(@"^[a-z]{0,3}\d{8,10}$").WithMessage(Errors.WrittenContent.InvalidLccnFormat.Description)
            .When(x => x.LCCN is not null);
        RuleFor(x => x.OCLCNumber)
            .Matches(@"^(ocm\d{8}|ocn\d{9,}|on\d{10,}|\(OCoLC\)\d{8,}|\d{8,})$").When(x => x.OCLCNumber is not null)
            .WithMessage(Errors.WrittenContent.InvalidOclcFormat.Description);
        RuleFor(x => x.OpenLibraryId)
            .Matches(@"^OL[1-9]\d*[AMW]$").When(x => x.OpenLibraryId is not null)
            .WithMessage(Errors.WrittenContent.InvalidOpenLibraryId.Description);
        RuleFor(x => x.LibraryThingId)
            .MaximumLength(50).When(x => x.LibraryThingId is not null)
            .WithMessage(Errors.WrittenContent.LibraryThingIdMustBeMaximum50CharactersLong.Description);
        RuleFor(x => x.GoogleBooksId)
            .Length(12).WithMessage(Errors.WrittenContent.GoogleBooksIdMustBe12CharactersLong.Description)
            .Matches(@"^[A-Za-z0-9_-]{12}$").WithMessage(Errors.WrittenContent.InvalidGoogleBooksIdFormat.Description)
            .When(x => x.GoogleBooksId is not null);
        RuleFor(x => x.BarnesAndNobleId)
            .Length(10).WithMessage(Errors.WrittenContent.BarnesAndNoblesIdMustBe10CharactersLong.Description)
            .Matches(@"^\d{10}$").WithMessage(Errors.WrittenContent.InvalidBarnesAndNoblesIdFormat.Description)
            .When(x => x.BarnesAndNobleId is not null);
        RuleFor(x => x.AppleBooksId)
            .Matches(@"^id\d+$").When(x => x.AppleBooksId is not null)
            .WithMessage(Errors.WrittenContent.InvalidAppleBooksIdFormat.Description);
        RuleFor(x => x.ISBNs)
            .NotNull().WithMessage(Errors.WrittenContent.IsbnListCannotBeNull.Description);
        RuleForEach(x => x.ISBNs)
            .ChildRules(isbn =>
            {
                isbn.RuleFor(i => i.Value)
                    .NotEmpty().WithMessage(Errors.WrittenContent.IsbnValueCannotBeEmpty.Description)
                    .Matches(i => i.Format == IsbnFormat.Isbn13
                        ? @"^(?:ISBN(?:-13)?:? )?(?=[0-9]{13}$|(?=(?:[0-9]+[-\ ]){4})[-\ 0-9]{17}$)97[89][-\ ]?[0-9]{1,5}[-\ ]?[0-9]+[-\ ]?[0-9]+[-\ ]?[0-9]$"
                        : @"^(?:ISBN(?:-10)?:? )?(?=[0-9X]{10}$|(?=(?:[0-9]+[-\ ]){3})[-\ 0-9X]{13}$)[0-9]{1,5}[-\ ]?[0-9]+[-\ ]?[0-9]+[-\ ]?[0-9X]$")
                    .WithMessage(i => i.Format == IsbnFormat.Isbn13
                        ? Errors.WrittenContent.InvalidIsbn13Format.Description
                        : Errors.WrittenContent.InvalidIsbn10Format.Description);
                isbn.RuleFor(i => i.Format)
                    .IsInEnum().WithMessage(Errors.WrittenContent.UnknownIsbnFormat.Description);
            });
        RuleFor(x => x.Contributors)
            .NotNull().WithMessage(Errors.MediaContributor.ContributorsListCannotBeNull.Description);
        RuleForEach(x => x.Contributors)
            .ChildRules(contributor =>
            {
                contributor.RuleFor(c => c.Name)
                    .NotNull().WithMessage(Errors.MediaContributor.ContributorNameCannotBeEmpty.Description)
                    .ChildRules(name =>
                    {
                        name.RuleFor(n => n!.DisplayName)
                            .NotNull().WithMessage(Errors.MediaContributor.ContributorDisplayNameCannotBeEmpty.Description)
                            .NotEmpty().WithMessage(Errors.MediaContributor.ContributorDisplayNameCannotBeEmpty.Description)
                            .MaximumLength(100).WithMessage(Errors.MediaContributor.ContributorDisplayNameMustBeMaximum100CharactersLong.Description);
                        name.RuleFor(n => n!.LegalName)
                            .MaximumLength(100).When(n => n!.LegalName is not null)
                            .WithMessage(Errors.MediaContributor.ContributorLegalNameMustBeMaximum100CharactersLong.Description);
                    });
                contributor.RuleFor(c => c.Role)
                    .NotNull().WithMessage(Errors.MediaContributor.ContributorRoleCannotBeNull.Description)
                    .ChildRules(role =>
                    {
                        role.RuleFor(r => r!.Name)
                            .NotEmpty().WithMessage(Errors.MediaContributor.RoleNameCannotBeEmpty.Description)
                            .MaximumLength(50).WithMessage(Errors.MediaContributor.RoleNameMustBeMaximum50CharactersLong.Description);
                        role.RuleFor(r => r!.Category)
                            .NotEmpty().WithMessage(Errors.MediaContributor.RoleCategoryCannotBeEmpty.Description)
                            .MaximumLength(50).WithMessage(Errors.MediaContributor.RoleCategoryMustBeMaximum50CharactersLong.Description);
                    });
            });
        RuleFor(x => x.Ratings)
            .NotNull().WithMessage(Errors.Metadata.RatingsListCannotBeNull.Description);
        RuleForEach(x => x.Ratings)
            .ChildRules(rating =>
            {
                rating.RuleFor(r => r.Value)
                    .GreaterThan(0).WithMessage(Errors.Metadata.RatingValueMustBePositive.Description)
                    .Must((r, value) => value <= r.MaxValue).WithMessage(Errors.Metadata.RatingValueCannotBeGreaterThanMaxValue.Description);
                rating.RuleFor(r => r.MaxValue)
                    .GreaterThan(0).WithMessage(Errors.Metadata.RatingMaxValueMustBePositive.Description);
                rating.RuleFor(r => r.VoteCount)
                    .GreaterThanOrEqualTo(0).When(r => r.VoteCount.HasValue)
                    .WithMessage(Errors.Metadata.RatingVoteCountMustBePositive.Description);
            });
    }
}
