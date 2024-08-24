#region ========================================================================= USING =====================================================================================
using FluentValidation;
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
        RuleFor(x => x.Metadata).NotNull()
            .ChildRules(metadata =>
            {
                metadata.RuleFor(m => m.Title).NotEmpty().MaximumLength(255);
                metadata.RuleFor(m => m.OriginalTitle).MaximumLength(255);
                metadata.RuleFor(m => m.Description).MaximumLength(2000);
                metadata.RuleFor(m => m.ReleaseInfo).NotNull()
                    .ChildRules(ri =>
                    {
                        ri.RuleFor(r => r.OriginalReleaseYear).InclusiveBetween(1, 9999).When(r => r.OriginalReleaseYear.HasValue);
                        ri.RuleFor(r => r.ReReleaseYear).InclusiveBetween(1, 9999).When(r => r.ReReleaseYear.HasValue);
                        ri.RuleFor(r => r.ReleaseCountry).MaximumLength(50);
                        ri.RuleFor(r => r.ReleaseVersion).MaximumLength(50);
                    });
                metadata.RuleFor(m => m.Genres).NotEmpty();
                metadata.RuleForEach(m => m.Genres).ChildRules(g => g.RuleFor(genre => genre.Name).NotEmpty().MaximumLength(50));
                metadata.RuleForEach(m => m.Tags).ChildRules(t => t.RuleFor(tag => tag.Name).NotEmpty().MaximumLength(50));
                metadata.RuleFor(m => m.Language).ChildRules(l =>
                {
                    l.RuleFor(x => x.LanguageCode).NotEmpty().MaximumLength(10);
                    l.RuleFor(x => x.LanguageName).NotEmpty().MaximumLength(50);
                    l.RuleFor(x => x.NativeName).MaximumLength(50);
                }).When(m => m.Language != null);
                metadata.RuleFor(m => m.OriginalLanguage).ChildRules(l =>
                {
                    l.RuleFor(x => x.LanguageCode).NotEmpty().MaximumLength(10);
                    l.RuleFor(x => x.LanguageName).NotEmpty().MaximumLength(50);
                    l.RuleFor(x => x.NativeName).MaximumLength(50);
                }).When(m => m.OriginalLanguage != null);
                metadata.RuleFor(m => m.Publisher).MaximumLength(100);
                metadata.RuleFor(m => m.PageCount).GreaterThan(0).When(m => m.PageCount.HasValue);
            });

        RuleFor(x => x.Format).IsInEnum();
        RuleFor(x => x.Edition).MaximumLength(50);
        RuleFor(x => x.VolumeNumber).GreaterThan(0).When(x => x.VolumeNumber.HasValue);

        RuleFor(x => x.Series).ChildRules(s => s.RuleFor(series => series.Title).NotEmpty().MaximumLength(255)).When(x => x.Series != null);

        RuleFor(x => x.ASIN).MaximumLength(10);
        RuleFor(x => x.GoodreadsId).MaximumLength(20);
        RuleFor(x => x.LCCN).MaximumLength(20);
        RuleFor(x => x.OCLCNumber).MaximumLength(20);
        RuleFor(x => x.OpenLibraryId).MaximumLength(20);
        RuleFor(x => x.LibraryThingId).MaximumLength(20);
        RuleFor(x => x.GoogleBooksId).MaximumLength(20);
        RuleFor(x => x.BarnesAndNobleId).MaximumLength(20);
        RuleFor(x => x.KoboId).MaximumLength(20);
        RuleFor(x => x.AppleBooksId).MaximumLength(20);

        RuleFor(x => x.ISBNs).NotEmpty();
        RuleForEach(x => x.ISBNs).ChildRules(isbn =>
        {
            isbn.RuleFor(i => i.Value).NotEmpty().MaximumLength(13);
            isbn.RuleFor(i => i.Format).IsInEnum();
        });

        RuleFor(x => x.Contributors).NotEmpty();
        RuleForEach(x => x.Contributors).ChildRules(contributor =>
        {
            contributor.RuleFor(c => c.Name).NotEmpty().MaximumLength(100);
            contributor.RuleFor(c => c.Role).ChildRules(role =>
            {
                role.RuleFor(r => r.Name).NotEmpty().MaximumLength(50);
                role.RuleFor(r => r.Category).NotEmpty().MaximumLength(50);
            });
        });

        RuleForEach(x => x.Ratings).ChildRules(rating =>
        {
            rating.RuleFor(r => r.Value).Must((r, value) => value >= 0 && value <= r.MaxValue)
                .WithMessage("Value must be between 0 and MaxValue");
            rating.RuleFor(r => r.MaxValue).GreaterThan(0);
            rating.RuleFor(r => r.VoteCount).GreaterThanOrEqualTo(0).When(r => r.VoteCount.HasValue);
        });
    }
    #endregion
}