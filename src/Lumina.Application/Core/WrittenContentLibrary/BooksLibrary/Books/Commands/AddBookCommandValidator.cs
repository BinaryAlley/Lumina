#region ========================================================================= USING =====================================================================================
using FluentValidation;
using Lumina.Domain.Common.Errors;
#endregion

namespace Lumina.Application.Core.WrittenContentLibrary.BooksLibrary.Books.Commands;

/// <summary>
/// Validates the needed validation rules for <see cref="AddBookCommand"/>.
/// </summary>
public class AddBookCommandValidator : AbstractValidator<AddBookCommand>
{
    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="AddBookCommandValidator"/> class.
    /// </summary>
    public AddBookCommandValidator()
    {
        RuleFor(x => x.Metadata)
            .NotNull().WithMessage(Errors.Metadata.MetadataCannotBeNull.Code)
            .ChildRules(metadata =>
            {
                metadata.RuleFor(m => m.Title)
                    .NotEmpty().WithMessage(Errors.Metadata.TitleCannotBeEmpty.Code)
                    .MaximumLength(255).WithMessage(Errors.Metadata.TitleMustBeMaximum255CharactersLong.Code);
                metadata.RuleFor(m => m.OriginalTitle)
                    .MaximumLength(255).When(m => m.OriginalTitle is not null)
                    .WithMessage(Errors.Metadata.OriginalTitleMustBeMaximum255CharactersLong.Code);
                metadata.RuleFor(m => m.Description)
                    .MaximumLength(2000).When(m => m.Description is not null)
                    .WithMessage(Errors.Metadata.DescriptionMustBeMaximum2000CharactersLong.Code);
                metadata.RuleFor(m => m.ReleaseInfo)
                    .NotNull().WithMessage(Errors.Metadata.ReleaseInfoCannotBeNull.Code)
                    .ChildRules(ri =>
                    {
                        ri.RuleFor(r => r.OriginalReleaseYear)
                            .InclusiveBetween(1, 9999).When(r => r.OriginalReleaseYear.HasValue)
                            .WithMessage(Errors.Metadata.OriginalReleaseYearMustBeBetween1And9999.Code);
                        ri.RuleFor(r => r.ReReleaseYear)
                            .InclusiveBetween(1, 9999).When(r => r.ReReleaseYear.HasValue)
                            .WithMessage(Errors.Metadata.ReReleaseYearMustBeBetween1And9999.Code);
                        ri.RuleFor(r => r.ReleaseCountry)
                            .Matches("^[A-Za-z]{2}$").When(r => r.ReleaseCountry is not null)
                            .WithMessage(Errors.Metadata.CountryCodeMustBe2CharactersLong.Code);
                        ri.RuleFor(r => r.ReleaseVersion)
                            .MaximumLength(50).When(r => r.ReleaseVersion is not null)
                            .WithMessage(Errors.Metadata.ReleaseVersionMustBeMaximum50CharactersLong.Code);
                        ri.RuleFor(r => r.ReReleaseYear)
                            .Must((releaseInfo, reReleaseYear) =>
                                !releaseInfo.ReReleaseYear.HasValue ||
                                !releaseInfo.OriginalReleaseYear.HasValue ||
                                reReleaseYear >= releaseInfo.OriginalReleaseYear)
                            .When(r => r.ReReleaseYear.HasValue && r.OriginalReleaseYear.HasValue)
                            .WithMessage(Errors.Metadata.ReReleaseYearCannotBeEarlierThanOriginalReleaseYear.Code);
                        ri.RuleFor(r => r.ReReleaseDate)
                            .Must((releaseInfo, reReleaseDate) =>
                                !releaseInfo.ReReleaseDate.HasValue ||
                                !releaseInfo.OriginalReleaseDate.HasValue ||
                                reReleaseDate >= releaseInfo.OriginalReleaseDate)
                            .When(r => r.ReReleaseDate.HasValue && r.OriginalReleaseDate.HasValue)
                            .WithMessage(Errors.Metadata.ReReleaseDateCannotBeEarlierThanOriginalReleaseDate.Code);
                    });
                metadata.RuleFor(m => m.Genres)
                    .NotNull().WithMessage(Errors.Metadata.GenresListCannotBeNull.Code);
                metadata.RuleForEach(m => m.Genres)
                    .ChildRules(g => 
                        g.RuleFor(genre => genre.Name)
                            .NotEmpty().WithMessage(Errors.Metadata.GenreNameCannotBeEmpty.Code)
                            .MaximumLength(50)).WithMessage(Errors.Metadata.GenreNameMustBeMaximum50CharactersLong.Code);
                metadata.RuleFor(m => m.Tags)
                    .NotNull().WithMessage(Errors.Metadata.TagsListCannotBeNull.Code);
                metadata.RuleForEach(m => m.Tags)
                    .ChildRules(t => 
                        t.RuleFor(tag => tag.Name)
                            .NotEmpty().WithMessage(Errors.Metadata.TagNameCannotBeEmpty.Code)
                            .MaximumLength(50).WithMessage(Errors.Metadata.TagNameMustBeMaximum50CharactersLong.Code));
                metadata.RuleFor(m => m.Language)
                    .ChildRules(l =>
                    {
                        l.RuleFor(x => x!.LanguageCode)
                            .NotEmpty().WithMessage(Errors.Metadata.LanguageCodeCannotBeEmpty.Code)
                            .MaximumLength(2).WithMessage(Errors.Metadata.LanguageCodeMustBe2CharactersLong.Code);
                        l.RuleFor(x => x!.LanguageName)
                            .NotEmpty().WithMessage(Errors.Metadata.LanguageNameCannotBeEmpty.Code)
                            .MaximumLength(50).WithMessage(Errors.Metadata.LanguageNameMustBeMaximum50CharactersLong.Code);
                        l.RuleFor(x => x!.NativeName)
                            .MaximumLength(50).When(x => x!.NativeName is not null)
                            .WithMessage(Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong.Code);
                    }).When(m => m.Language is not null);
                metadata.RuleFor(m => m.OriginalLanguage)
                    .ChildRules(l =>
                    {
                        l.RuleFor(x => x!.LanguageCode)
                            .NotEmpty().WithMessage(Errors.Metadata.LanguageCodeCannotBeEmpty.Code)
                            .MaximumLength(2).WithMessage(Errors.Metadata.LanguageCodeMustBe2CharactersLong.Code);
                        l.RuleFor(x => x!.LanguageName)
                            .NotEmpty().WithMessage(Errors.Metadata.LanguageNameCannotBeEmpty.Code)
                            .MaximumLength(50).WithMessage(Errors.Metadata.LanguageNameMustBeMaximum50CharactersLong.Code);
                        l.RuleFor(x => x!.NativeName)
                            .MaximumLength(50).When(x => x!.NativeName is not null)
                            .WithMessage(Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong.Code);
                    }).When(m => m.OriginalLanguage is not null);
                metadata.RuleFor(m => m.Publisher)
                    .MaximumLength(100).When(m => m.Publisher is not null)
                    .WithMessage(Errors.WrittenContent.PublisherMustBeMaximum100CharactersLong.Code);
                metadata.RuleFor(m => m.PageCount)
                    .GreaterThan(0).When(m => m.PageCount.HasValue)
                    .WithMessage(Errors.WrittenContent.PageCountMustBeGreaterThanZero.Code);
            });
        RuleFor(x => x.Format)
            .IsInEnum().WithMessage(Errors.WrittenContent.UnknownBookFormat.Code);
        RuleFor(x => x.Edition)
            .MaximumLength(50).When(x => x.Edition is not null)
            .WithMessage(Errors.WrittenContent.EditionMustBeMaximum50CharactersLong.Code);
        RuleFor(x => x.VolumeNumber)
            .GreaterThan(0).When(x => x.VolumeNumber.HasValue)
            .WithMessage(Errors.WrittenContent.VolumeNumberMustBeGreaterThanZero.Code);
        RuleFor(x => x.Series)
            .ChildRules(s => 
                s.RuleFor(series => series!.Title)
                    .NotEmpty().WithMessage(Errors.Metadata.TitleCannotBeEmpty.Code)
                    .MaximumLength(255)).WithMessage(Errors.Metadata.TitleMustBeMaximum255CharactersLong.Code)
            .When(x => x.Series is not null);
        RuleFor(x => x.ASIN)
            .Length(10).When(x => x.ASIN is not null)
            .WithMessage(Errors.WrittenContent.AsinMustBe10CharactersLong.Code);
        RuleFor(x => x.GoodreadsId)
            .Matches(@"^\d+$").When(x => x.GoodreadsId is not null)
            .WithMessage(Errors.WrittenContent.GoodreadsIdMustBeNumeric.Code); 
        RuleFor(x => x.LCCN)
            .Matches(@"^[a-z]{0,3}\d{8,10}$").WithMessage(Errors.WrittenContent.InvalidLccnFormat.Code)
            .When(x => x.LCCN is not null);
        RuleFor(x => x.OCLCNumber)
            .Matches(@"^(ocm\d{8}|ocn\d{9,}|on\d{10,}|\(OCoLC\)\d{8,}|\d{8,})$").When(x => x.OCLCNumber is not null)
            .WithMessage(Errors.WrittenContent.InvalidOclcFormat.Code);
        RuleFor(x => x.OpenLibraryId)
            .Matches(@"^OL[1-9]\d*[AMW]$").When(x => x.OpenLibraryId is not null)
            .WithMessage(Errors.WrittenContent.InvalidOpenLibraryId.Code);
        RuleFor(x => x.LibraryThingId)
            .MaximumLength(50).When(x => x.LibraryThingId is not null)
            .WithMessage(Errors.WrittenContent.LibraryThingIdMustBeMaximum50CharactersLong.Code);
        RuleFor(x => x.GoogleBooksId)
            .Length(12).WithMessage(Errors.WrittenContent.GoogleBooksIdMustBe12CharactersLong.Code)
            .Matches(@"^[A-Za-z0-9_-]{12}$").WithMessage(Errors.WrittenContent.InvalidGoogleBooksIdFormat.Code)
            .When(x => x.GoogleBooksId is not null);
        RuleFor(x => x.BarnesAndNobleId)
            .Length(10).WithMessage(Errors.WrittenContent.BarnesAndNoblesIdMustBe10CharactersLong.Code)
            .Matches(@"^\d{10}$").WithMessage(Errors.WrittenContent.InvalidBarnesAndNoblesIdFormat.Code)
            .When(x => x.BarnesAndNobleId is not null);
        RuleFor(x => x.AppleBooksId)
            .Matches(@"^id\d+$").When(x => x.AppleBooksId is not null)
            .WithMessage(Errors.WrittenContent.InvalidAppleBooksIdFormat.Code);
        RuleFor(x => x.ISBNs)
            .NotNull().WithMessage(Errors.WrittenContent.IsbnListCannotBeNull.Code);
        RuleForEach(x => x.ISBNs)
            .ChildRules(isbn =>
            {
                isbn.RuleFor(i => i.Value)
                    .NotEmpty().WithMessage(Errors.WrittenContent.IsbnValueCannotBeEmpty.Code)
                    .MaximumLength(13).WithMessage(Errors.WrittenContent.InvalidIsbn13Format.Code);
                isbn.RuleFor(i => i.Format)
                    .IsInEnum().WithMessage(Errors.WrittenContent.UnknownIsbnFormat.Code);
            });
        RuleFor(x => x.Contributors)
            .NotNull().WithMessage(Errors.MediaContributor.ContributorsListCannotBeNull.Code);
        RuleForEach(x => x.Contributors)
            .ChildRules(contributor =>
            {
                contributor.RuleFor(c => c.Name)
                    .NotEmpty().WithMessage(Errors.MediaContributor.ContributorNameCannotBeEmpty.Code)
                    .MaximumLength(100).WithMessage(Errors.MediaContributor.ContributorNameMustBeMaximum100CharactersLong.Code);
                contributor.RuleFor(c => c.Role)
                    .NotNull().WithMessage(Errors.MediaContributor.ContributorRoleCannotBeNull.Code)  
                    .ChildRules(role =>
                    {
                        role.RuleFor(r => r.Name)
                            .NotEmpty().WithMessage(Errors.MediaContributor.RoleNameCannotBeEmpty.Code)
                            .MaximumLength(50).WithMessage(Errors.MediaContributor.RoleNameMustBeMaximum50CharactersLong.Code);
                        role.RuleFor(r => r.Category)
                            .NotEmpty().WithMessage(Errors.MediaContributor.RoleCategoryCannotBeEmpty.Code)
                            .MaximumLength(50).WithMessage(Errors.MediaContributor.RoleCategoryMustBeMaximum50CharactersLong.Code);
                    });
            });
        RuleFor(x => x.Ratings)
            .NotNull().WithMessage(Errors.Metadata.RatingsListCannotBeNull.Code);
        RuleForEach(x => x.Ratings)
            .ChildRules(rating =>
            {
                rating.RuleFor(r => r.Value)
                    .GreaterThan(0).WithMessage(Errors.Metadata.RatingValueMustBePositive.Code)
                    .Must((r, value) => value <= r.MaxValue).WithMessage(Errors.Metadata.RatingValueCannotBeGreaterThanMaxValue.Code);
                rating.RuleFor(r => r.MaxValue)
                    .GreaterThan(0).WithMessage(Errors.Metadata.RatingMaxValueMustBePositive.Code);
                rating.RuleFor(r => r.VoteCount)
                    .GreaterThanOrEqualTo(0).When(r => r.VoteCount.HasValue)
                    .WithMessage(Errors.Metadata.RatingVoteCountMustBePositive.Code);
            });
    }
    #endregion
}