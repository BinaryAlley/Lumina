#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
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
        RuleFor(command => command.LibraryId)
            .NotEmpty()
            .WithError(Errors.WrittenContent.BookLibraryCannotBeNull);
       
        RuleFor(command => command.Path)
            .NotEmpty()
            .WithError(Errors.WrittenContent.BookPathCannotBeEmpty);
       
        RuleFor(command => command.Metadata)
            .NotNull()
            .WithError(Errors.Metadata.MetadataCannotBeNull)
            .ChildRules(metadata =>
            {
                metadata.RuleFor(m => m!.Title)
                    .NotEmpty()
                    .WithError(Errors.Metadata.TitleCannotBeEmpty)
                    .MaximumLength(255)
                    .WithError(Errors.Metadata.TitleMustBeMaximum255CharactersLong);
                
                metadata.RuleFor(m => m!.OriginalTitle)
                    .MaximumLength(255)
                    .When(m => m!.OriginalTitle is not null)
                    .WithError(Errors.Metadata.OriginalTitleMustBeMaximum255CharactersLong);
                
                metadata.RuleFor(m => m!.Description)
                    .MaximumLength(2000)
                    .When(m => m!.Description is not null)
                    .WithError(Errors.Metadata.DescriptionMustBeMaximum2000CharactersLong);
               
                metadata.RuleFor(m => m!.ReleaseInfo)
                    .NotNull()
                    .WithError(Errors.Metadata.ReleaseInfoCannotBeNull)
                    .ChildRules(releaseInfo =>
                    {
                        releaseInfo.RuleFor(r => r!.OriginalReleaseYear)
                            .InclusiveBetween(1, 9999)
                            .When(r => r!.OriginalReleaseYear.HasValue)
                            .WithError(Errors.Metadata.OriginalReleaseYearMustBeBetween1And9999);
                        
                        releaseInfo.RuleFor(r => r!.ReReleaseYear)
                            .InclusiveBetween(1, 9999)
                            .When(r => r!.ReReleaseYear.HasValue)
                            .WithError(Errors.Metadata.ReReleaseYearMustBeBetween1And9999);
                       
                        releaseInfo.RuleFor(r => r!.ReleaseCountry)
                            .Matches("^[A-Za-z]{2}$")
                            .When(r => r!.ReleaseCountry is not null)
                            .WithError(Errors.Metadata.CountryCodeMustBe2CharactersLong);
                       
                        releaseInfo.RuleFor(r => r!.ReleaseVersion)
                            .MaximumLength(50)
                            .When(r => r!.ReleaseVersion is not null)
                            .WithError(Errors.Metadata.ReleaseVersionMustBeMaximum50CharactersLong);
                        
                        releaseInfo.RuleFor(r => r!.OriginalReleaseYear)
                            .Must((releaseInfoInstance, originalReleaseYear) =>
                                !releaseInfoInstance!.OriginalReleaseDate.HasValue ||
                                !releaseInfoInstance.OriginalReleaseYear.HasValue ||
                                originalReleaseYear == releaseInfoInstance.OriginalReleaseDate.Value.Year)
                            .When(r => r!.OriginalReleaseDate.HasValue && r.OriginalReleaseYear.HasValue)
                            .WithError(Errors.Metadata.OriginalReleaseDateAndYearMustMatch);
                       
                        releaseInfo.RuleFor(r => r!.ReReleaseYear)
                            .Must((releaseInfoInstance, reReleaseYear) =>
                                !releaseInfoInstance!.ReReleaseDate.HasValue ||
                                !releaseInfoInstance.ReReleaseYear.HasValue ||
                                reReleaseYear == releaseInfoInstance.ReReleaseDate.Value.Year)
                            .When(r => r!.ReReleaseDate.HasValue && r.ReReleaseYear.HasValue)
                            .WithError(Errors.Metadata.ReReleaseDateAndYearMustMatch);
                       
                        releaseInfo.RuleFor(r => r!.ReReleaseYear)
                            .Must((releaseInfoInstance, reReleaseYear) =>
                                !releaseInfoInstance!.ReReleaseYear.HasValue ||
                                !releaseInfoInstance.OriginalReleaseYear.HasValue ||
                                reReleaseYear >= releaseInfoInstance.OriginalReleaseYear)
                            .When(r => r!.ReReleaseYear.HasValue && r.OriginalReleaseYear.HasValue)
                            .WithError(Errors.Metadata.ReReleaseYearCannotBeEarlierThanOriginalReleaseYear);
                       
                        releaseInfo.RuleFor(r => r!.ReReleaseDate)
                            .Must((releaseInfoInstance, reReleaseDate) =>
                                !releaseInfoInstance!.ReReleaseDate.HasValue ||
                                !releaseInfoInstance.OriginalReleaseDate.HasValue ||
                                reReleaseDate >= releaseInfoInstance.OriginalReleaseDate)
                            .When(r => r!.ReReleaseDate.HasValue && r.OriginalReleaseDate.HasValue)
                            .WithError(Errors.Metadata.ReReleaseDateCannotBeEarlierThanOriginalReleaseDate);
                     
                        releaseInfo.RuleFor(r => r!.ReReleaseYear)
                            .Must((releaseInfoInstance, reReleaseYear) =>
                                !releaseInfoInstance!.ReReleaseDate.HasValue ||
                                !releaseInfoInstance.ReReleaseYear.HasValue ||
                                reReleaseYear == releaseInfoInstance.ReReleaseDate.Value.Year)
                            .When(r => r!.ReReleaseDate.HasValue && r.ReReleaseYear.HasValue)
                            .WithError(Errors.Metadata.ReReleaseDateAndYearMustMatch);
                    });
             
                metadata.RuleFor(m => m!.Genres)
                    .NotNull()
                    .WithError(Errors.Metadata.GenresListCannotBeNull);
              
                metadata.RuleForEach(m => m!.Genres)
                    .ChildRules(genre =>
                        genre.RuleFor(g => g.Name)
                            .NotEmpty()
                            .WithError(Errors.Metadata.GenreNameCannotBeEmpty)
                            .MaximumLength(50)
                            .WithError(Errors.Metadata.GenreNameMustBeMaximum50CharactersLong));
              
                metadata.RuleFor(m => m!.Tags)
                    .NotNull()
                    .WithError(Errors.Metadata.TagsListCannotBeNull);
             
                
                metadata.RuleForEach(m => m!.Tags)
                    .ChildRules(tag =>
                        tag.RuleFor(t => t.Name)
                            .NotEmpty()
                            .WithError(Errors.Metadata.TagNameCannotBeEmpty)
                            .MaximumLength(50)
                            .WithError(Errors.Metadata.TagNameMustBeMaximum50CharactersLong));
            
                metadata.RuleFor(m => m!.Language!.LanguageCode)
                    .NotEmpty()
                    .WithError(Errors.Metadata.LanguageCodeCannotBeEmpty)
                    .MaximumLength(2)
                    .WithError(Errors.Metadata.LanguageCodeMustBe2CharactersLong)
                    .When(m => m!.Language is not null);
              
                metadata.RuleFor(m => m!.Language!.LanguageName)
                    .NotEmpty()
                    .WithError(Errors.Metadata.LanguageNameCannotBeEmpty)
                    .MaximumLength(50)
                    .WithError(Errors.Metadata.LanguageNameMustBeMaximum50CharactersLong)
                    .When(m => m!.Language is not null);
              
                metadata.RuleFor(m => m!.Language!.NativeName)
                    .MaximumLength(50)
                    .WithError(Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong)
                    .When(m => m!.Language is not null);
             
                metadata.RuleFor(m => m!.OriginalLanguage!.LanguageCode)
                    .NotEmpty().WithError(Errors.Metadata.LanguageCodeCannotBeEmpty)
                    .MaximumLength(2)
                    .WithError(Errors.Metadata.LanguageCodeMustBe2CharactersLong)
                    .When(m => m!.OriginalLanguage is not null);
            
                metadata.RuleFor(m => m!.OriginalLanguage!.LanguageName)
                    .NotEmpty()
                    .WithError(Errors.Metadata.LanguageNameCannotBeEmpty)
                    .MaximumLength(50)
                    .WithError(Errors.Metadata.LanguageNameMustBeMaximum50CharactersLong)
                    .When(m => m!.OriginalLanguage is not null);
           
                metadata.RuleFor(m => m!.OriginalLanguage!.NativeName)
                    .MaximumLength(50)
                    .WithError(Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong)
                    .When(m => m!.OriginalLanguage is not null);
           
                metadata.RuleFor(m => m!.Publisher)
                    .MaximumLength(100)
                    .When(m => m!.Publisher is not null)
                    .WithError(Errors.WrittenContent.PublisherMustBeMaximum100CharactersLong);
           
                metadata.RuleFor(m => m!.PageCount)
                    .GreaterThan(0)
                    .When(m => m!.PageCount.HasValue)
                    .WithError(Errors.WrittenContent.PageCountMustBeGreaterThanZero);
            });
       
        RuleFor(command => command.Format)
            .IsInEnum()
            .When(command => command.Format is not null)
            .WithError(Errors.WrittenContent.UnknownBookFormat);
     
        RuleFor(command => command.Edition)
            .MaximumLength(50)
            .When(command => command.Edition is not null)
            .WithError(Errors.WrittenContent.EditionMustBeMaximum50CharactersLong);
      
        RuleFor(command => command.VolumeNumber)
            .GreaterThan(0)
            .When(command => command.VolumeNumber.HasValue)
            .WithError(Errors.WrittenContent.VolumeNumberMustBeGreaterThanZero);
             
        RuleFor(command => command.Series)
            .ChildRules(series =>
                series.RuleFor(s => s!.Title)
                    .NotEmpty()
                    .WithError(Errors.Metadata.TitleCannotBeEmpty)
                    .MaximumLength(255)
                    .WithError(Errors.Metadata.TitleMustBeMaximum255CharactersLong))
            .When(command => command.Series is not null);
      
        RuleFor(command => command.ASIN)
            .Length(10)
            .When(command => command.ASIN is not null)
            .WithError(Errors.WrittenContent.AsinMustBe10CharactersLong);
      
        RuleFor(command => command.GoodreadsId)
            .Matches(@"^\d+$")
            .When(command => command.GoodreadsId is not null)
            .WithError(Errors.WrittenContent.GoodreadsIdMustBeNumeric);
     
        RuleFor(command => command.LCCN)
            .Matches(@"^[a-z]{0,3}\d{8,10}$")
            .When(command => command.LCCN is not null)
            .WithError(Errors.WrittenContent.InvalidLccnFormat);
      
        RuleFor(command => command.OCLCNumber)
            .Matches(@"^(ocm\d{8}|ocn\d{9,}|on\d{10,}|\(OCoLC\)\d{8,}|\d{8,})$").When(command => command.OCLCNumber is not null)
            .WithError(Errors.WrittenContent.InvalidOclcFormat);
      
        RuleFor(command => command.OpenLibraryId)
            .Matches(@"^OL[1-9]\d*[AMW]$")
            .When(command => command.OpenLibraryId is not null)
            .WithError(Errors.WrittenContent.InvalidOpenLibraryId);
     
        RuleFor(command => command.LibraryThingId)
            .MaximumLength(50)
            .When(command => command.LibraryThingId is not null)
            .WithError(Errors.WrittenContent.LibraryThingIdMustBeMaximum50CharactersLong);
     
        RuleFor(command => command.GoogleBooksId)
            .Length(12)
            .When(command => command.GoogleBooksId is not null)
            .WithError(Errors.WrittenContent.GoogleBooksIdMustBe12CharactersLong)
            .Matches(@"^[A-Za-z0-9_-]{12}$")
            .When(command => command.GoogleBooksId is not null)
            .WithError(Errors.WrittenContent.InvalidGoogleBooksIdFormat);
      
        RuleFor(command => command.BarnesAndNobleId)
            .Length(10)
            .When(command => command.BarnesAndNobleId is not null)
            .WithError(Errors.WrittenContent.BarnesAndNoblesIdMustBe10CharactersLong)
            .Matches(@"^\d{10}$")
            .When(command => command.BarnesAndNobleId is not null)
            .WithError(Errors.WrittenContent.InvalidBarnesAndNoblesIdFormat);
     
        RuleFor(command => command.AppleBooksId)
            .Matches(@"^id\d+$")
            .When(command => command.AppleBooksId is not null)
            .WithError(Errors.WrittenContent.InvalidAppleBooksIdFormat);
     
        RuleFor(command => command.ISBNs)
            .NotNull()
            .WithError(Errors.WrittenContent.IsbnListCannotBeNull);
       
        RuleForEach(command => command.ISBNs)
            .ChildRules(isbn =>
            {
                isbn.RuleFor(i => i.Value)
                    .NotEmpty()
                    .WithError(Errors.WrittenContent.IsbnValueCannotBeEmpty);
                
                isbn.RuleFor(i => i.Value)
                    .Matches(@"^(?:ISBN(?:-13)?:? )?(?=[0-9]{13}$|(?=(?:[0-9]+[-\ ]){4})[-\ 0-9]{17}$)97[89][-\ ]?[0-9]{1,5}[-\ ]?[0-9]+[-\ ]?[0-9]+[-\ ]?[0-9]$")
                    .When(i => i.Format == IsbnFormat.Isbn13)
                    .WithError(Errors.WrittenContent.InvalidIsbn13Format);
          
                isbn.RuleFor(i => i.Value)
                    .Matches(@"^(?:ISBN(?:-10)?:? )?(?=[0-9X]{10}$|(?=(?:[0-9]+[-\ ]){3})[-\ 0-9X]{13}$)[0-9]{1,5}[-\ ]?[0-9]+[-\ ]?[0-9]+[-\ ]?[0-9X]$")
                    .When(i => i.Format == IsbnFormat.Isbn10)
                    .WithError(Errors.WrittenContent.InvalidIsbn10Format);
             
                isbn.RuleFor(i => i.Format)
                    .IsInEnum()
                    .WithError(Errors.WrittenContent.UnknownIsbnFormat);
            });
      
        RuleFor(command => command.Contributors)
            .NotNull()
            .WithError(Errors.MediaContributor.ContributorsListCannotBeNull);
      
        RuleForEach(command => command.Contributors)
            .ChildRules(contributor =>
            {
                contributor.RuleFor(c => c.Name)
                    .NotNull()
                    .WithError(Errors.MediaContributor.ContributorNameCannotBeEmpty)
                    .ChildRules(name =>
                    {
                        name.RuleFor(n => n!.DisplayName)
                            .NotNull()
                            .WithError(Errors.MediaContributor.ContributorDisplayNameCannotBeEmpty)
                            .NotEmpty()
                            .WithError(Errors.MediaContributor.ContributorDisplayNameCannotBeEmpty)
                            .MaximumLength(100)
                            .WithError(Errors.MediaContributor.ContributorDisplayNameMustBeMaximum100CharactersLong);
                      
                        name.RuleFor(n => n!.LegalName)
                            .MaximumLength(100)
                            .When(n => n!.LegalName is not null)
                            .WithError(Errors.MediaContributor.ContributorLegalNameMustBeMaximum100CharactersLong);
                    });
               
                contributor.RuleFor(c => c.Role)
                    .NotNull()
                    .WithError(Errors.MediaContributor.ContributorRoleCannotBeNull)
                    .ChildRules(role =>
                    {
                        role.RuleFor(r => r!.Name)
                            .NotEmpty()
                            .WithError(Errors.MediaContributor.RoleNameCannotBeEmpty)
                            .MaximumLength(50)
                            .WithError(Errors.MediaContributor.RoleNameMustBeMaximum50CharactersLong);
                        role.RuleFor(r => r!.Category)
                            .NotEmpty()
                            .WithError(Errors.MediaContributor.RoleCategoryCannotBeEmpty)
                            .MaximumLength(50)
                            .WithError(Errors.MediaContributor.RoleCategoryMustBeMaximum50CharactersLong);
                    });
            });
       
        RuleFor(command => command.Ratings)
            .NotNull()
            .WithError(Errors.Metadata.RatingsListCannotBeNull);
      
        RuleForEach(command => command.Ratings)
            .ChildRules(rating =>
            {
                rating.RuleFor(r => r.Value)
                    .GreaterThan(0)
                    .WithError(Errors.Metadata.RatingValueMustBePositive)
                    .Must((ratingInstance, value) => value <= ratingInstance.MaxValue)
                    .WithError(Errors.Metadata.RatingValueCannotBeGreaterThanMaxValue);
             
                rating.RuleFor(r => r.MaxValue)
                    .GreaterThan(0)
                    .WithError(Errors.Metadata.RatingMaxValueMustBePositive);
            
                rating.RuleFor(r => r.VoteCount)
                    .GreaterThanOrEqualTo(0)
                    .When(r => r.VoteCount.HasValue)
                    .WithError(Errors.Metadata.RatingVoteCountMustBePositive);
            });
    }
}
